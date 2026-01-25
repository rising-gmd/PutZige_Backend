// Update RateLimitingExtensions: use ErrorMessages.RateLimit.* for user-facing messages
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PutZige.Application.Settings;
using PutZige.Application.Validators;
using FluentValidation;
using FluentValidation.Results;
using System.Threading.Tasks;
using PutZige.Application.Common.Messages;

namespace PutZige.API.Extensions
{
    /// <summary>
    /// Extensions to register rate limiting using built-in ASP.NET Core rate limiting.
    /// </summary>
    public static class RateLimitingExtensions
    {
        private const string LoginPolicyName = "login";
        private const string RefreshTokenPolicyName = "refresh-token";
        private const string RegistrationPolicyName = "registration";
        private const string GlobalPolicyName = "global-api";

        /// <summary>
        /// Register rate limiting services and configuration.
        /// </summary>
        public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind and validate settings using FluentValidation
            var section = configuration.GetSection(RateLimitSettings.SectionName);
            var settings = section.Get<RateLimitSettings>() ?? new RateLimitSettings();

            // Validate using validator; if validation fails, disable rate limiting and log error later
            var validator = new RateLimitSettingsValidator();
            ValidationResult? result = null;
            try
            {
                result = validator.Validate(settings);
            }
            catch (Exception ex)
            {
                var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
                loggerFactory?.CreateLogger("RateLimiting").LogError(ex, "Exception while validating rate limit settings");
                settings.Enabled = false;
            }

            if (result != null && !result.IsValid)
            {
                var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger("RateLimiting");
                foreach (var failure in result.Errors)
                {
                    logger?.LogError("RateLimitSettings validation failure: {Property} - {Error}", failure.PropertyName, failure.ErrorMessage);
                }

                // Log a concise validation-failure message and disable rate limiting
                logger?.LogError(ErrorMessages.RateLimit.ValidationFailed);
                settings.Enabled = false;
            }

            // Register settings for DI
            services.Configure<RateLimitSettings>(section);

            // Always register the IOptions validator in case other code checks
            services.AddSingleton<IValidator<RateLimitSettings>, RateLimitSettingsValidator>();

            // If disabled, just return (services are registered but limiter not added)
            if (!settings.Enabled)
            {
                services.AddSingleton(settings);
                var sp = services.BuildServiceProvider();
                sp.GetService<ILoggerFactory>()?.CreateLogger("RateLimiting").LogWarning(ErrorMessages.RateLimit.Disabled);
                return services;
            }

            // Configure RateLimiter
            services.AddRateLimiter(options =>
            {
                var logger = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("RateLimiting");

                try
                {
                    options.AddPolicy(LoginPolicyName, _ =>
                        RateLimitPartition.GetFixedWindowLimiter("login", _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.Login.PermitLimit,
                            Window = TimeSpan.FromSeconds(settings.Login.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                    options.AddPolicy(RefreshTokenPolicyName, _ =>
                        RateLimitPartition.GetFixedWindowLimiter("refresh-token", _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.RefreshToken.PermitLimit,
                            Window = TimeSpan.FromSeconds(settings.RefreshToken.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                    options.AddPolicy(RegistrationPolicyName, _ =>
                        RateLimitPartition.GetFixedWindowLimiter("registration", _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.Registration.PermitLimit,
                            Window = TimeSpan.FromSeconds(settings.Registration.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to register named fixed-window policies. Disabling rate limiting.");
                    return;
                }

                // Global limiter partitioned by user id or ip using sliding window
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    try
                    {
                        // If endpoint has an explicit EnableRateLimiting attribute, bypass global limiter
                        var endpoint = httpContext.GetEndpoint();
                        if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute>() != null)
                        {
                            return RateLimitPartition.GetNoLimiter("bypass");
                        }

                        string? userId = httpContext.User?.FindFirst("sub")?.Value;
                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            var xff = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(xff))
                            {
                                var first = xff.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                                if (!string.IsNullOrWhiteSpace(first))
                                {
                                    userId = NormalizeIp(first);
                                }
                            }

                            if (string.IsNullOrWhiteSpace(userId))
                            {
                                userId = httpContext.Connection.RemoteIpAddress?.ToString();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            userId = "unknown";
                        }

                        var slidingOptions = new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = settings.GlobalApi.PermitLimit,
                            Window = TimeSpan.FromSeconds(settings.GlobalApi.WindowSeconds),
                            SegmentsPerWindow = settings.GlobalApi.SegmentsPerWindow,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        };

                        return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => slidingOptions);
                    }
                    catch (Exception ex)
                    {
                        var logger2 = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("RateLimiting");
                        logger2?.LogError(ex, "Exception while creating rate limit partition; failing open for availability.");
                        return RateLimitPartition.GetNoLimiter("fail-open");
                    }
                });

                // Rejection handling
                options.OnRejected = async (context, cancellationToken) =>
                {
                    try
                    {
                        var logger2 = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("RateLimiting");
                        var httpContext = context.HttpContext;
                        var endpoint = httpContext?.GetEndpoint();
                        var policyName = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute>()?.PolicyName ?? GlobalPolicyName;

                        var partitionKey = "unknown";
                        var userId = httpContext?.User?.FindFirst("sub")?.Value;
                        if (!string.IsNullOrWhiteSpace(userId)) partitionKey = userId;
                        else
                        {
                            var xff = httpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(xff)) partitionKey = NormalizeIp(xff.Split(',').FirstOrDefault() ?? "");
                            else partitionKey = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
                        }

                        var limit = settings.GlobalApi.PermitLimit;
                        var window = settings.GlobalApi.WindowSeconds;

                        logger2?.LogWarning("Rate limit exceeded: Policy={PolicyName}, Endpoint={Endpoint}, Partition={Partition}, Limit={Limit}, Window={WindowSeconds}s, Algorithm={Algorithm}",
                            policyName, endpoint?.DisplayName, partitionKey, limit, window, "SlidingWindow");

                        var retryAfter = 0;

                        var payload = new
                        {
                            error = ErrorMessages.RateLimit.Exceeded,
                            retryAfter = retryAfter,
                            policyName = policyName,
                            limit = limit,
                            window = window
                        };

                        var response = httpContext?.Response;
                        if (response != null)
                        {
                            response.StatusCode = StatusCodes.Status429TooManyRequests;
                            response.Headers["Retry-After"] = retryAfter.ToString();
                            response.ContentType = "application/json";
                            await response.WriteAsync(JsonSerializer.Serialize(payload), cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger2 = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("RateLimiting");
                        logger2?.LogError(ex, "Exception in OnRejected handler; allowing request through");
                    }
                };
            });

            var sp2 = services.BuildServiceProvider();
            sp2.GetService<ILoggerFactory>()?.CreateLogger("RateLimiting").LogInformation("Rate limiting registered: Global {Limit}/{Window}s segments={Segments}", settings.GlobalApi.PermitLimit, settings.GlobalApi.WindowSeconds, settings.GlobalApi.SegmentsPerWindow);

            return services;
        }

        private static string NormalizeIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return ip ?? string.Empty;
            var idx = ip.IndexOf(':');
            if (idx > 0 && ip.Count(c => c == ':') == 1)
            {
                return ip.Substring(0, idx);
            }

            return ip.Trim();
        }
    }
}

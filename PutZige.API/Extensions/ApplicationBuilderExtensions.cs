// Update ApplicationBuilderExtensions to reference Application.Settings
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PutZige.Application.Settings;

namespace PutZige.API.Extensions
{
    /// <summary>
    /// Application builder extensions for rate limiting middleware.
    /// Middleware ordering: Authentication MUST run before rate limiting so user id can be extracted from JWT.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the built-in rate limiting middleware if rate limiting is enabled in configuration.
        /// Ensure this is called AFTER app.UseAuthentication() and BEFORE app.UseAuthorization().
        /// </summary>
        public static IApplicationBuilder UseRateLimitingMiddleware(this IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<Microsoft.Extensions.Options.IOptions<RateLimitSettings>>()?.Value;
            var logger = app.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger("RateLimiting");
            if (settings == null)
            {
                logger?.LogWarning("RateLimitSettings not configured; skipping rate limiter middleware.");
                return app;
            }

            if (!settings.Enabled)
            {
                logger?.LogInformation("Rate limiting is disabled in configuration; skipping rate limiter middleware.");
                return app;
            }

            // Use built-in middleware
            app.UseRateLimiter();
            logger?.LogInformation("Rate limiting middleware registered.");
            return app;
        }
    }
}

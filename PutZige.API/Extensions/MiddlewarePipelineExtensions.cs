using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using PutZige.Application.Settings;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.AspNetCore;

namespace PutZige.API.Extensions
{
    /// <summary>
    /// Configure middleware ordering and pipeline composition.
    /// </summary>
    public static class MiddlewarePipelineExtensions
    {
        /// <summary>
        /// Configure middleware order: Serilog request logging -> Global exception handler -> Swagger (dev) -> Routing -> HTTPS -> Authentication -> Rate limiting -> Authorization -> MapControllers.
        /// </summary>
        public static void ConfigureMiddlewarePipeline(this WebApplication app)
        {
            var logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("MiddlewarePipeline");

            // Request logging
            app.UseSerilogRequestLogging();

            // Global exception handling
            app.UseMiddleware<PutZige.API.Middleware.GlobalExceptionHandlerMiddleware>();

            // Swagger only in Development
            app.UseSwaggerConfiguration();

            app.UseRouting();

            // CORS must be applied after routing and before authentication/authorization
            app.UseCorsConfiguration();

            app.UseHttpsRedirection();

            // Authentication must happen before rate limiting so rate limiter can use authenticated user id where available
            app.UseAuthentication();

            // Rate limiting should be after authentication but before authorization to avoid performing authorization work for requests that will be rejected by rate limits
            // Use built-in rate limiter via ApplicationBuilderExtensions or call UseRateLimiter directly
            app.UseRateLimitingMiddleware();

            app.UseAuthorization();

            app.MapControllers();

            logger?.LogInformation("Middleware pipeline configured");
        }
    }
}

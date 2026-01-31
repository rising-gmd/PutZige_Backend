using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace PutZige.API.Extensions
{
    /// <summary>
    /// Swagger configuration extensions.
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Adds Swagger generation configuration.
        /// </summary>
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            try
            {
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PutZige API", Version = "v1" });
                });
            }
            catch (Exception ex)
            {
                var logger = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("Swagger");
                logger?.LogError(ex, "Failed to configure Swagger; continuing without Swagger");
            }

            return services;
        }

        /// <summary>
        /// Enable Swagger middleware in Development and mount UI at application root for developer convenience.
        /// </summary>
        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetService<IHostEnvironment>();
            var logger = app.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger("Swagger");

            try
            {
                if (env != null && env.IsDevelopment())
                {
                    // Serve the generated Swagger JSON and mount the UI at the application root ('/')
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PutZige API v1");
                        // Expose swagger UI at root to make developer experience frictionless during local development
                        c.RoutePrefix = string.Empty;
                    });

                    logger?.LogInformation("Swagger UI enabled at application root in Development environment");
                }
                else
                {
                    logger?.LogDebug("Swagger is disabled in non-development environment");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Exception while configuring Swagger middleware; continuing without Swagger");
            }

            return app;
        }
    }
}

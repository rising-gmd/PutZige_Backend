using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace PutZige.API.Extensions
{
    /// <summary>
    /// Serilog logging configuration extensions.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configure Serilog host logging and enrichers from IConfiguration.
        /// </summary>
        public static void ConfigureLogging(this WebApplicationBuilder builder)
        {
            try
            {
                // Configure Serilog as the host logger using configuration if present.
                builder.Host.UseSerilog((context, services, configuration) =>
                {
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithProperty("Application", "PutZige")
                        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
                });
            }
            catch (Exception ex)
            {
                // If Serilog fails to configure, fall back to bootstrap logger and log the issue.
                var logger = Log.Logger;
                logger?.Warning(ex, "Failed to fully configure Serilog; continuing with bootstrap logger");
            }
        }
    }
}

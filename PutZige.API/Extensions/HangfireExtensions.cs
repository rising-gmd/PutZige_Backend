using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Hangfire.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using PutZige.API.Filters;

namespace PutZige.API.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration config)
    {
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(config.GetConnectionString("Default")));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 5;
            options.ServerName = "PutZige-EmailWorker";
        });

        return services;
    }

    public static void ConfigureHangfireDashboard(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
    }
}

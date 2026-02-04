using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Hangfire.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using PutZige.API.Filters;
using PutZige.Infrastructure.Settings;

namespace PutZige.API.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>()?.ConnectionString;

        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString));

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

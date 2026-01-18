#nullable enable
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Data;
using PutZige.Infrastructure.Repositories;
using PutZige.Infrastructure.Settings;
using PutZige.Infrastructure.DependencyInjectionHelpers;
using System;
using System.Linq;

namespace PutZige.Infrastructure;

/// <summary>
/// Registers infrastructure services into the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services such as DbContext, repositories and settings validators.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Bind DatabaseSettings using options pattern and validate with FluentValidation
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        //services.AddSingleton<IValidator<DatabaseSettings>, DatabaseSettingsValidator>();
        //services.AddSingleton<IValidateOptions<DatabaseSettings>, DatabaseSettingsOptionsValidator>();

        // Register DbContext with resilient SQL Server settings
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

            options.UseSqlServer(
                dbSettings.ConnectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: dbSettings.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(dbSettings.CommandTimeout);
                });

            if (environment.IsDevelopment())
            {
                if (dbSettings.EnableSensitiveDataLogging)
                    options.EnableSensitiveDataLogging();

                if (dbSettings.EnableDetailedErrors)
                    options.EnableDetailedErrors();
            }
        });

        // Register repositories and unit of work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

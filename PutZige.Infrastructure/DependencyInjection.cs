// File: src/PutZige.Infrastructure/DependencyInjection.cs

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
using System.Linq;

namespace PutZige.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Register and validate DatabaseSettings
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        services.AddSingleton<IValidator<DatabaseSettings>, DatabaseSettingsValidator>();

        // Validate settings on startup
        var serviceProvider = services.BuildServiceProvider();
        var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        var validator = serviceProvider.GetRequiredService<IValidator<DatabaseSettings>>();
        var validationResult = validator.Validate(dbSettings);

        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException($"DatabaseSettings validation failed: {errors}");
        }

        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                dbSettings.ConnectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: dbSettings.MaxRetryCount,
                        maxRetryDelay: System.TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(dbSettings.CommandTimeout);
                });

            // Development-only features
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

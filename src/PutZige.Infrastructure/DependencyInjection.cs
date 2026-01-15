// File: src/PutZige.Infrastructure/DependencyInjection.cs

using System;
using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Configuration;
using PutZige.Infrastructure.Data;
using PutZige.Infrastructure.Repositories;

namespace PutZige.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection(DatabaseSettings.SectionName))
            .ValidateFluently()
            .ValidateOnStart();

        services.AddSingleton<IValidator<DatabaseSettings>, DatabaseSettingsValidator>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            options.UseSqlServer(
                dbSettings.ConnectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        dbSettings.MaxRetryCount,
                        TimeSpan.FromSeconds(dbSettings.CommandTimeout),
                        null);
                });
            if (dbSettings.EnableSensitiveDataLogging)
                options.EnableSensitiveDataLogging();
            if (dbSettings.EnableDetailedErrors)
                options.EnableDetailedErrors();
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // Validate settings on startup
        using (var scope = services.BuildServiceProvider().CreateScope())
        {
            var settings = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<DatabaseSettings>>();
            var result = validator.Validate(settings);
            if (!result.IsValid)
            {
                throw new OptionsValidationException(
                    DatabaseSettings.SectionName,
                    typeof(DatabaseSettings),
                    result.Errors.ConvertAll(e => e.ErrorMessage));
            }
        }

        return services;
    }

    private static OptionsBuilder<TOptions> ValidateFluently<TOptions>(this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        builder.Services.AddSingleton<IValidateOptions<TOptions>>(sp =>
            new FluentValidationOptions<TOptions>(sp.GetRequiredService<IValidator<TOptions>>()));
        return builder;
    }

    private class FluentValidationOptions<TOptions> : IValidateOptions<TOptions> where TOptions : class
    {
        private readonly IValidator<TOptions> _validator;
        public FluentValidationOptions(IValidator<TOptions> validator) => _validator = validator;
        public ValidateOptionsResult Validate(string? name, TOptions options)
        {
            var result = _validator.Validate(options);
            if (result.IsValid) return ValidateOptionsResult.Success;
            var errors = result.Errors.ConvertAll(e => e.ErrorMessage);
            return ValidateOptionsResult.Fail(errors);
        }
    }
}

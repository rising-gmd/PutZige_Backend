// File: src/PutZige.Application/DependencyInjection.cs

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using PutZige.Application.Services;
using PutZige.Application.Interfaces;

namespace PutZige.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register application services
        services.AddScoped<UserService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}

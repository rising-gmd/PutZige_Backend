// File: src/PutZige.Application/DependencyInjection.cs

using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PutZige.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        // Register application services here, e.g.:
        // services.AddScoped<IUserService, UserService>();
        return services;
    }
}

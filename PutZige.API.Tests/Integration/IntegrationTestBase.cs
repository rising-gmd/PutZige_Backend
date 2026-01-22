// PutZige.API.Tests/Integration/IntegrationTestBase.cs
#nullable enable
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PutZige.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PutZige.API.Tests.Integration
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;

        protected IntegrationTestBase()
        {
            var dbName = Guid.NewGuid().ToString();
            Factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove any existing DbContext registrations to avoid multiple providers
                        var descriptors = services.Where(d =>
                            (d.ServiceType != null && d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                            d.ServiceType == typeof(AppDbContext) ||
                            (d.ImplementationType != null && d.ImplementationType == typeof(AppDbContext))
                        ).ToList();

                        foreach (var descriptor in descriptors)
                            services.Remove(descriptor);

                        // Create a dedicated EF service provider for InMemory to isolate provider services
                        var efServiceProvider = new ServiceCollection()
                            .AddEntityFrameworkInMemoryDatabase()
                            .BuildServiceProvider();

                        // Add in-memory database for testing and set internal service provider
                        services.AddDbContext<AppDbContext>(options =>
                        {
                            options.UseInMemoryDatabase(dbName);
                            options.UseInternalServiceProvider(efServiceProvider);
                        });
                    });
                });

            Client = Factory.CreateClient();
        }

        public void Dispose()
        {
            Client.Dispose();
            Factory.Dispose();
        }
    }
}

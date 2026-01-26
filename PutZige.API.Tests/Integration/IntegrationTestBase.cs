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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

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
                    builder.ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        // Provide explicit rate limit settings for tests to ensure binding succeeds
                        var overrides = new Dictionary<string, string?>
                        {
                            ["RateLimitSettings:Enabled"] = "true",
                            ["RateLimitSettings:Login:PermitLimit"] = "5",
                            ["RateLimitSettings:Login:WindowSeconds"] = "900",
                            ["RateLimitSettings:RefreshToken:PermitLimit"] = "10",
                            ["RateLimitSettings:RefreshToken:WindowSeconds"] = "900",
                            ["RateLimitSettings:Registration:PermitLimit"] = "3",
                            ["RateLimitSettings:Registration:WindowSeconds"] = "3600",
                            ["RateLimitSettings:GlobalApi:PermitLimit"] = "1000",
                            ["RateLimitSettings:GlobalApi:WindowSeconds"] = "60",
                            ["RateLimitSettings:GlobalApi:SegmentsPerWindow"] = "8",
                            ["RateLimitSettings:UseDistributedCache"] = "false"
                        };

                        // Provide minimal JWT settings for authentication to be enabled in tests
                        var jwtSecret = "this_is_a_test_secret_key_at_least_32_chars!";
                        overrides["JwtSettings:Secret"] = jwtSecret;
                        overrides["JwtSettings:Issuer"] = "putzige-test";
                        overrides["JwtSettings:Audience"] = "putzige-test-audience";
                        overrides["JwtSettings:AccessTokenExpiryMinutes"] = "60";
                        overrides["JwtSettings:RefreshTokenExpiryDays"] = "7";

                        configBuilder.AddInMemoryCollection(overrides!);
                    });

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

                        // Ensure a named policy required by controllers exists in test environment
                        // Some controllers use "api-general" policy name; provide a no-op bypass policy for tests
                        services.PostConfigure<RateLimiterOptions>(opts =>
                        {
                            try
                            {
                                opts.AddPolicy("api-general", httpContext => RateLimitPartition.GetNoLimiter("test-bypass"));
                            }
                            catch
                            {
                                // ignore if policy already exists
                            }
                        });

                        // Add a test authentication scheme that accepts a simple bearer token or JWT for tests.
                        services.AddAuthentication().AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, PutZige.API.Tests.Integration.TestAuthHandler>("Test", _ => { });

                        // Ensure the test scheme is the default so the app's JwtBearer doesn't override it in tests
                        services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(opts =>
                        {
                            opts.DefaultAuthenticateScheme = "Test";
                            opts.DefaultChallengeScheme = "Test";
                        });
                    });
                });

            Client = Factory.CreateClient();
            // Ensure each test client has a unique partition key for rate limiting
            Client.DefaultRequestHeaders.Add("X-Test-Client", dbName);
        }

        public void Dispose()
        {
            Client.Dispose();
            Factory.Dispose();
        }
    }
}

# PutZige.API.Tests - Integration & End-to-End HTTP Tests

## What We Test

- Full HTTP request/response pipeline
- Endpoints such as `POST /api/v1/auth/register`
- Validation, error responses, and database state changes
- NO mocking of the HTTP pipeline (use in-memory host)

## How It Works

- `WebApplicationFactory<TEntryPoint>` hosts the API in-memory
- Replace real DB with in-memory `AppDbContext` for deterministic tests
- Use `HttpClient` provided by the factory to make real HTTP calls

## IntegrationTestBase

A base class to centralize WebApplicationFactory and test DB setup.

```csharp
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace DbContext with in-memory version
            });
        });

        Client = Factory.CreateClient();
    }
}
```

## Example: POST /api/v1/auth/register Test

```csharp
public class AuthControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_ReturnsCreated_AndPersistsUser()
    {
        var dto = new { Email = "test@domain.com", Password = "P@ssw0rd" };
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        body.Should().NotBeNull();

        // verify database state
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == "test@domain.com");
        user.Should().NotBeNull();
    }
}
```

## Running Tests

```
dotnet test PutZige.API.Tests
```

Run a single integration test with filter:

```
dotnet test --filter "FullyQualifiedName~AuthControllerTests.Register_ReturnsCreated"
```

## Best Practices

- Test the full pipeline including middleware and filters
- Replace external services with test implementations when necessary
- Verify both HTTP response and database side effects
- Keep tests isolated: reset DB between tests

- Don't mock the controllers or middleware; prefer testing end-to-end

## Troubleshooting

- If tests fail due to DI registration differences, align test service configuration with production registrations
- Ensure DB is reset between tests (use unique DB or cleanup scripts)
- For flaky network-like behavior, increase timeouts or add retry logic selectively

## Quick Checklist

- [ ] WebApplicationFactory configured
- [ ] In-memory DB replacement in place
- [ ] HTTP client used for assertions

## Summary

API tests exercise the full application stack in-memory and validate both HTTP behavior and persistence. Keep these tests realistic and focused on user-facing scenarios such as registration flows and error handling.

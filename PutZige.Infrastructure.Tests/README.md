# PutZige.Infrastructure.Tests - Repository & Database Tests

## What We Test

- Repository implementations (e.g. `UserRepository`) and EF Core queries
- CRUD operations and query correctness
- Migrations and SQL-specific behavior (when using Testcontainers)
- NO mocking for repository behavior (use real or in-memory DB)

## Two Approaches

1. In-Memory EF Core (90%) - fast, no Docker
   - Use `UseInMemoryDatabase` for quick tests
   - Great for CRUD, mapping, and basic queries

2. Testcontainers (10%) - real SQL Server in Docker
   - Use `Testcontainers` for integration tests that require real SQL semantics
   - Requires Docker Desktop

## DatabaseTestBase

A base class to centralize setup and teardown for tests. Use a unique DB name per test to avoid cross-test contamination.

```csharp
public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected DbContextOptions<AppDbContext> Options { get; }

    protected DatabaseTestBase()
    {
        Options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}
```

## Example: Repository Test (In-Memory)

```csharp
public class UserRepositoryTests : DatabaseTestBase
{
    [Fact]
    public async Task AddUser_SavesToDatabase()
    {
        using var context = new AppDbContext(Options);
        var repo = new UserRepository(context);

        var user = new User("id", "test@example.com");
        await repo.AddAsync(user);
        await context.SaveChangesAsync();

        var saved = await context.Users.FindAsync(user.Id);
        saved.Should().NotBeNull();
        saved.Email.Should().Be("test@example.com");
    }
}
```

## Example: Testcontainers (SQL Server)

```csharp
// Pseudocode
var container = new MsSqlTestcontainer(new TestcontainersConfiguration { /*...*/ });
await container.StartAsync();
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(container.ConnectionString)
    .Options;
// Run migrations and tests against real SQL Server
await container.StopAsync();
```

## Running Tests

```
dotnet test PutZige.Infrastructure.Tests
```

To run Testcontainers tests (Docker required):

```
# Ensure Docker Desktop is running
dotnet test PutZige.Infrastructure.Tests --filter TestCategory=Integration
```

## Best Practices

- Use a unique DB per test (`Guid.NewGuid()`)
- Call `SaveChangesAsync()` in tests that persist
- Dispose contexts after use
- Keep most tests on in-memory provider and only use Testcontainers for SQL-specific behaviors

- Do not rely on shared database state across tests

## Troubleshooting

- InMemory provider does not simulate SQL behavior (e.g. transactions, relational constraints). Use Testcontainers for such scenarios.
- Ensure migrations are applied for Testcontainers runs before assertions

## Quick Checklist

- [ ] Database per test
- [ ] SaveChangesAsync called
- [ ] Context disposed
- [ ] Testcontainers tests guarded behind Docker check

## Summary

`PutZige.Infrastructure.Tests` validates repository layer correctness. Favor fast in-memory tests for most scenarios and reserve Testcontainers for migration and SQL-specific verifications.

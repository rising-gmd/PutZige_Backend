# PutZige.Infrastructure

Infrastructure layer implementing data access, repository implementations, and external service integrations. This project depends on `PutZige.Application` (for interfaces) and `PutZige.Domain`.

## Table of contents

- [Principles](#principles)
- [Structure](#structure)
- [Database](#database)
- [Migrations](#migrations)
- [Configuration](#configuration)
- [Service registration](#service-registration)
- [Dependencies](#dependencies)
- [Usage examples](#usage-examples)
- [Related READMEs](#related-readmes)

## Principles

- Implements application layer interfaces
- Keep EF Core specifics inside this project
- Provide dependency injection extension methods for registration in the API

## Structure

```
PutZige.Infrastructure/
??? Data/
?   ??? ApplicationDbContext.cs
?   ??? Configurations/        # EF entity configurations
?   ??? Migrations/            # EF migrations
??? Repositories/              # Repository implementations
??? Services/                  # External service implementations
??? DependencyInjection.cs     # Service registration
```

Exact paths:

- `PutZige.Infrastructure/Data/ApplicationDbContext.cs`
- `PutZige.Infrastructure/Data/Configurations/`
- `PutZige.Infrastructure/Data/Migrations/`
- `PutZige.Infrastructure/Repositories/`
- `PutZige.Infrastructure/Services/`
- `PutZige.Infrastructure/DependencyInjection.cs`

## Database

- Provider: SQL Server (primary) — PostgreSQL may be supported in code but is not the default unless configured.
- ORM: Entity Framework Core (EF Core)

Connection string used by runtime: read from `PutZige.API/appsettings.json` via `DefaultConnection` key.

### Migrations
Add a migration (exact commands):

```bash
# from solution root
dotnet ef migrations add <Name> --project PutZige.Infrastructure/PutZige.Infrastructure.csproj --startup-project PutZige.API/PutZige.API.csproj
```

Apply migrations:

```bash
dotnet ef database update --project PutZige.Infrastructure/PutZige.Infrastructure.csproj --startup-project PutZige.API/PutZige.API.csproj
```

Migrations folder: `PutZige.Infrastructure/Data/Migrations/` (create if scaffolded).

## Service registration

Call from `PutZige.API/Program.cs`:

```csharp
services.AddInfrastructure(configuration);
```

`AddInfrastructure` method is expected at `PutZige.Infrastructure/DependencyInjection.cs` and registers `ApplicationDbContext`, repositories, and external services.

## Dependencies
Check `PutZige.Infrastructure/PutZige.Infrastructure.csproj` for exact PackageReference entries. Common packages used here:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`

## Usage examples

Resolve a repository from DI in application code:

```csharp
// in a service or handler
var user = await _userRepository.GetByEmailAsync(email);
```

Add DbContext in `Program.cs` (example):

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

## Related READMEs
- Root README: `../README.md`
- API README: `../PutZige.API/README.md`

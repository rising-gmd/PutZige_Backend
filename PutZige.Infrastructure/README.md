# PutZige.Infrastructure

Infrastructure layer: data access, EF Core DbContext, repository implementations and configuration.

## Purpose
Provides persistence and external integrations required by application services.

## Contents

### Folder structure
```
PutZige.Infrastructure/
??? Data/
?   ??? AppDbContext.cs
?   ??? Configurations/
?   ?   ??? BaseEntityConfiguration.cs
?   ?   ??? UserConfiguration.cs
?   ??? Migrations/
??? Repositories/
?   ??? Repository.cs
?   ??? UserRepository.cs
??? Configuration/
?   ??? DatabaseSettings.cs
?   ??? DatabaseSettingsValidator.cs
??? DependencyInjection.cs
??? README.md
```

### Key files
- `Data/AppDbContext.cs` - EF Core `DbContext`, DbSets for `User`, `UserSettings`, `UserSession`, `UserRateLimit`, `UserMetadata`
- `Data/Configurations/*` - EF Core entity configurations and mapping rules
- `Data/Migrations/` - EF Core migrations (migration files)
- `Repositories/Repository.cs` - generic repository implementation
- `Repositories/UserRepository.cs` - user-specific queries and includes
- `Configuration/DatabaseSettings.cs` - options class for DB config
- `DependencyInjection.cs` - registers `AppDbContext`, repositories, and configuration

## DbContext features
- Automatic timestamps via `AuditableEntity` configuration
- Soft-delete support pattern implemented in base configuration
- Centralized entity configurations in `Configurations/`

## Connection string
- Primary location: `PutZige.API/appsettings.json` under `ConnectionStrings:Default`
- Can be overridden with environment variable `ConnectionStrings__Default`

## How to run migrations
From solution root:
- Add/Update migrations:
  - `dotnet ef migrations add <Name> --project PutZige.Infrastructure --startup-project PutZige.API`
- Apply migrations:
  - `dotnet ef database update --project PutZige.Infrastructure --startup-project PutZige.API`

## Dependencies
- Project refs: `PutZige.Domain`
- NuGet: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`

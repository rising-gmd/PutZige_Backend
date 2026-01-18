# PutZige.Infrastructure

Infrastructure layer: data access, EF Core `DbContext`, repository implementations and configuration.

## Purpose
Provides persistence and external integrations required by application services.

## Contents

### Folder structure
```
PutZige.Infrastructure/
?? Data/
?  ?? AppDbContext.cs            # EF Core DbContext with conventions
?  ?? Configurations/            # IEntityTypeConfiguration implementations
?  ?? Migrations/                # EF Core migration files
?? Repositories/
?  ?? Repository.cs              # Generic repository implementation (virtual methods)
?  ?? UserRepository.cs          # User-specific repository logic
?  ?? UnitOfWork.cs              # IUnitOfWork implementation
?? DependencyInjection.cs        # Registers DbContext, repositories, UoW and settings
?? DependencyInjectionHelpers/   # small helpers (options validator)
?? Settings/
?  ?? DatabaseSettings.cs
?  ?? DatabaseSettingsValidator.cs
?? README.md
```

### Key files and changes (latest)
- `Data/AppDbContext.cs` - EF Core `DbContext` with:
  - Automatic timestamps (`CreatedAt`, `UpdatedAt`, `DeletedAt`) applied in `SaveChangesAsync`.
  - Automatic `Guid` assignment for entities with empty `Id` when added.
  - Soft-delete conversion (DELETE -> soft-delete) in `SaveChangesAsync`.
  - Global query filter applied for `BaseEntity.IsDeleted`.
- `Repositories/Repository.cs` - generic repository:
  - Read-only queries use `AsNoTracking()` for performance.
  - Methods are `virtual` to enable overriding in specific repositories or tests.
  - `AddAsync`/`Update`/`Delete`/`HardDelete` follow soft-delete conventions.
- `Repositories/UnitOfWork.cs` - simple `IUnitOfWork` implementation delegating to `AppDbContext.SaveChangesAsync`.
- `DependencyInjection.cs` - registers `DatabaseSettings` via the Options pattern, registers a small `IValidateOptions<T>` helper and FluentValidation validator, and registers `AppDbContext` using the resolved settings. Signature is:
  - `AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)`

## DbContext features (detailed)
- Automatic timestamps and audit fields handled centrally in `SaveChangesAsync`.
- Soft-delete implemented via `BaseEntity.IsDeleted` and converted in the DB save pipeline.
- Global query filters ensure `IsDeleted == false` for all `BaseEntity`-derived entities.
- `AppDbContext` assigns a new `Guid` to `Id` when not provided on added entities. This avoids callers needing to set Ids manually before `AddAsync`.

## Repository patterns and behavior
- Generic `Repository<TEntity>` implements `IRepository<TEntity>` and is registered as `Scoped`.
- Read performance: queries that do not intend to modify entities use `AsNoTracking()`.
- Soft delete is the default behavior when calling `Delete(entity)`; `HardDelete(entity)` permanently removes the entity.
- `UnitOfWork` exists and is registered as `Scoped` to encapsulate `SaveChangesAsync` calls when needed.

## Connection string and settings
- Configuration bound to `Database` section via Options pattern (`DatabaseSettings`).
- Settings are validated at startup via both a FluentValidation validator and a lightweight `IValidateOptions<T>` implementation to fail fast if configuration is invalid.

## How to run migrations
From solution root:
- Add/Update migrations:
  - `dotnet ef migrations add <Name> --project PutZige.Infrastructure --startup-project PutZige.API`
- Apply migrations:
  - `dotnet ef database update --project PutZige.Infrastructure --startup-project PutZige.API`

## Dependencies
- Project refs: `PutZige.Domain`
- NuGet: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `FluentValidation`

## Notes and recommendations
- Services that will modify entities should use `IUnitOfWork.SaveChangesAsync` or the `AppDbContext` directly via DI to ensure `CreatedAt`/`UpdatedAt` are correctly applied.
- When a caller needs a tracked entity for further updates, use repository methods that don't call `AsNoTracking()` or fetch via the `DbContext` directly.
- Consider adding explicit integration tests for soft-delete/global query-filter behavior and for the automatic Guid assignment on add.

# PutZige.Domain

Domain layer: single source of truth for business entities and domain types. No external dependencies.

## Purpose
Holds core domain models, enums and repository interfaces. Contains only business concepts and rules.

## Contents

### Folder structure
```
PutZige.Domain/
??? Entities/
?   ??? AuditableEntity.cs
?   ??? BaseEntity.cs
?   ??? User.cs
?   ??? UserMetadata.cs
?   ??? UserRateLimit.cs
?   ??? UserSession.cs
?   ??? UserSettings.cs
??? Enums/
??? Interfaces/
??? README.md
```

### Key files
- `Entities/BaseEntity.cs` - base `Id` and common identity fields
- `Entities/AuditableEntity.cs` - audit fields (`CreatedAt`, `UpdatedAt`)
- `Entities/User.cs` - `User` aggregate root and core properties
- `Entities/UserSettings.cs` - user preference properties (1:1)
- `Entities/UserSession.cs` - session and presence tracking (1:1)
- `Entities/UserRateLimit.cs` - rate limiting fields (1:1)
- `Entities/UserMetadata.cs` - JSON metadata container (1:1)
- `Interfaces/IRepository<T>.cs` - generic repository contract
- `Interfaces/IUserRepository.cs` - user-specific repository contract

## Design patterns
- Base Entity Pattern (all entities inherit `BaseEntity`)
- Aggregate roots represented by `User` with related 1:1 entities

## Rules & conventions
- No references to EF Core, Infrastructure, or external libs
- Keep validation and persistence out of this project
- Mutable properties only where domain requires; prefer clear invariants on entities

## Dependencies
- No NuGet packages required for domain
- Referenced by `PutZige.Application` and `PutZige.Infrastructure`

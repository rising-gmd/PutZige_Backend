# PutZige.Domain

Core project that contains plain domain entities and repository/unit-of-work interfaces used across the solution. This is NOT a DDD/hexagonal implementation — it is a simple shared kernel of types and contracts consumed by Application and Infrastructure projects.

## Table of contents

- [What it contains](#what-it-contains)
- [Files and paths](#files-and-paths)
- [How to reference / import](#how-to-reference--import)
- [Build / restore](#build--restore)
- [Quick usage examples](#quick-usage-examples)
- [Dependencies](#dependencies)
- [Notes for maintainers](#notes-for-maintainers)

## What it contains

- Plain entity classes used by other projects (no framework-specific behaviors here).
- Repository and Unit of Work interfaces to be implemented by the Infrastructure project.
- Small base / auditable classes shared by entities.

This project targets `.NET 10` and is intended as a lightweight library of types and contracts.

## Files and paths (actual)

The following files and folders exist in the project root:

```
PutZige.Domain/
??? Entities/
?   ??? BaseEntity.cs
?   ??? AuditableEntity.cs
?   ??? User.cs
?   ??? UserSettings.cs
?   ??? UserSession.cs
?   ??? UserRateLimit.cs
?   ??? UserMetadata.cs
??? Interfaces/
?   ??? IRepository.cs
?   ??? IUnitOfWork.cs
?   ??? IUserRepository.cs
??? obj/                     # build artifacts
??? PutZige.Domain.csproj
```

Exact relative paths (copy/paste):
- `PutZige.Domain/Entities/User.cs`
- `PutZige.Domain/Entities/UserSettings.cs`
- `PutZige.Domain/Entities/UserSession.cs`
- `PutZige.Domain/Entities/UserRateLimit.cs`
- `PutZige.Domain/Entities/UserMetadata.cs`
- `PutZige.Domain/Entities/BaseEntity.cs`
- `PutZige.Domain/Entities/AuditableEntity.cs`
- `PutZige.Domain/Interfaces/IRepository.cs`
- `PutZige.Domain/Interfaces/IUnitOfWork.cs`
- `PutZige.Domain/Interfaces/IUserRepository.cs`

## How to reference / import

From another project in this solution add a project reference:

```bash
# from solution root or consumer project folder
dotnet add <ConsumerProject>.csproj reference PutZige.Domain/PutZige.Domain.csproj
```

In C# files import the namespaces and use types directly:

```csharp
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;

// example: use User type
var user = new User(/* constructor args as defined in code */);
```

If you rely on repository interfaces, they are implemented by the Infrastructure project. Resolve them via DI in consumers.

## Build / restore

Restore and build the domain project or the solution:

```bash
dotnet restore
dotnet build PutZige.Domain/PutZige.Domain.csproj
```

Run tests that depend on the domain types (test projects live in sibling folders):

```bash
dotnet test
```

## Quick usage examples

1) Add project reference to API or Infrastructure

```bash
dotnet add PutZige.API/PutZige.API.csproj reference PutZige.Domain/PutZige.Domain.csproj
```

2) Resolve and use `IUserRepository` in a handler (example signature):

```csharp
// constructor DI in an application handler or service
public class SomeHandler
{
    private readonly IUserRepository _userRepository;

    public SomeHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task DoSomethingAsync()
    {
        var user = await _userRepository.GetByIdAsync(Guid.Parse("..."));
        // business logic here
    }
}
```

Note: inspect `PutZige.Domain/Interfaces/IUserRepository.cs` for the exact method names and signatures before calling.

## Dependencies

- Target framework: `net10.0` (see `PutZige.Domain/PutZige.Domain.csproj`)
- The project has no external NuGet package references declared in the csproj; it is a plain class library.

## Notes for maintainers

- This project is intentionally minimal — keep it focused on plain types and interfaces. Do not introduce infrastructure details or third-party dependencies here unless absolutely required.
- Use file-scoped namespaces and modern C# idioms consistent with the solution.
- When adding public API surface, prefer stable method names and keep compatibility in mind (other projects depend on these contracts).
- Controllers and API code should not consume domain entities directly; map domain entities to DTOs in the Application layer before returning data to controllers or external consumers.
- If you want me to remove any remaining DDS/architectural wording from other READMEs, tell me which file(s) and I'll update them.

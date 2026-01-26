# PutZige.Infrastructure

Infrastructure layer implementing data access, repository implementations, and external service integrations. This project depends on `PutZige.Application` (for interfaces) and `PutZige.Domain`.

## Table of contents

- [Principles](#principles)
- [Structure](#structure)
- [Database](#database)
- [JWT support](#jwt-support)
- [Migrations](#migrations)
- [Configuration](#configuration)
- [Service registration](#service-registration)
- [Mapping guidance](#mapping-guidance)
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
  Data/
  Repositories/
  Services/
  Settings/
  DependencyInjection.cs
```

## Database

- Provider: SQL Server by default
- ORM: Entity Framework Core (EF Core)
- `AppDbContext` exposes `DbSet<User>`, `DbSet<UserSession>` and other domain entities.

## JWT support

This layer implements `IJwtTokenService` and depends on the `JwtSettings` type defined in the Application layer.

- `PutZige.Infrastructure/Services/JwtTokenService.cs` generates signed JWT access tokens and cryptographically-random refresh tokens.
- The `JwtTokenService` is registered in DI by `DependencyInjection.AddInfrastructureServices` and expects `JwtSettings` to be configured in app configuration under the `JwtSettings` section.

Configuration example (appsettings.json):
```json
"JwtSettings": {
  "Secret": "your-256-bit-secret-key-minimum-32-characters-long",
  "Issuer": "PutZige",
  "Audience": "PutZige.Users",
  "AccessTokenExpiryMinutes": 15,
  "RefreshTokenExpiryDays": 7
}
```

Security notes:
- Use a secret store or environment variables to supply `JwtSettings:Secret` in production; do not commit secrets to source control.

## Migrations

Add migration and update database using EF CLI (example):
```bash
dotnet ef migrations add AddUserSession --project PutZige.Infrastructure --startup-project PutZige.API
dotnet ef database update --project PutZige.Infrastructure --startup-project PutZige.API
```

## Service registration

Call from `PutZige.API/Program.cs`:
```csharp
services.AddInfrastructureServices(configuration, environment);
```

This registers `AppDbContext`, repositories, and `JwtTokenService` along with `JwtSettings` configuration.

Messaging and SignalR:
- `MessageRepository` implements message persistence and optimized conversation queries.
- `CurrentUserService` and `ClientInfoService` expose request-scoped metadata and current user claims.
- SignalR configuration is available via `SignalRSettings` in app configuration and is wired in the API project.

## Dependencies

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`

## Usage examples

Request a token from the Auth controller and the `JwtTokenService` will sign tokens using configured settings.

## Related READMEs
- `../PutZige.Application/README.md`
- `../PutZige.API/README.md`

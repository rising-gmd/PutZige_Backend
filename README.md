# PutZige

PutZige is a messaging application implemented with Clean Architecture. The solution focuses on user management, sessions, preferences, rate limiting, and extensible user metadata.

## Purpose
HTTP entry point and solution-level overview for developers onboarding to the codebase.

## Architecture
- Four layers: `PutZige.Domain`, `PutZige.Application`, `PutZige.Infrastructure`, `PutZige.API` (presentation)
- Clean separation: Domain (pure models), Application (use-cases), Infrastructure (data/IO), API (HTTP)

## Tech stack
- .NET 10
- Entity Framework Core (SQL Server provider)
- FluentValidation
- `BCrypt.Net-Next` for password hashing

## Quick start
1. Restore dependencies:

   `dotnet restore`

2. Build solution:

   `dotnet build`

3. Run migrations (from solution root):

   `dotnet ef database update --project PutZige.Infrastructure --startup-project PutZige.API`

4. Run API:

   `dotnet run --project PutZige.API`

## Project structure
```
PutZige/
??? PutZige.Application/
??? PutZige.Domain/
??? PutZige.Infrastructure/
??? PutZige.API/
```

## Current features
- User aggregate with `UserSettings`, `UserSession`, `UserRateLimit`, `UserMetadata`
- Repository pattern with generic `IRepository<T>` and `IUserRepository`
- FluentValidation-based validators in the application layer
- Options pattern for configuration objects
- EF Core DbContext with entity configurations, automatic audit timestamps, and soft-delete pattern

## Database schema (overview)
- `Users` — authentication, profile, security
- `UserSettings` — preferences (1:1 with `User`)
- `UserSession` — session/presence tracking (1:1 with `User`)
- `UserRateLimit` — rate limiting state (1:1 with `User`)
- `UserMetadata` — JSON metadata (1:1 with `User`)

## How to run migrations
- Add migration:
  `dotnet ef migrations add <Name> --project PutZige.Infrastructure --startup-project PutZige.API`
- Apply migrations:
  `dotnet ef database update --project PutZige.Infrastructure --startup-project PutZige.API`

## Environment setup
- Configure `ConnectionStrings:Default` in `PutZige.API/appsettings.json` or set environment variable `ConnectionStrings__Default`.
- Use a local SQL Server instance or container. Apply migrations before running the API in production.

## Contacts
- Source: https://github.com/rising-gmd/PutZige (origin)

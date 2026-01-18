# PutZige.API

HTTP API for PutZige. Hosts controllers, middleware and application wiring.

## Purpose
Expose application use-cases over HTTP; handle request/response, DI and middleware pipeline.

## Contents

### Folder structure
```
PutZige.API/
?? Controllers/
?? Middleware/
?? Properties/
?? appsettings.json
?? Program.cs
?? PutZige.API.http
?? README.md
```

### Key files
- `Controllers/` - Web API controllers (endpoints that call `IUserService` and other application services)
- `Middleware/` - global error handler, logging middleware, and other HTTP middleware
- `Program.cs` - application startup: DI, middleware pipeline, Swagger, and configuration
- `appsettings.json` - environment configuration and `ConnectionStrings`

## Dependency Injection
- `Program.cs` calls:
  - `services.AddApplicationServices()` (registers `PutZige.Application`)
  - `services.AddInfrastructureServices(configuration, environment)` (registers `PutZige.Infrastructure` including `AppDbContext`)
- Controllers receive service interfaces via constructor injection

## Running the API
- From solution root: `dotnet run --project PutZige.API`
- Ensure DB migrations applied before starting in non-development environments
- The infrastructure layer now validates `DatabaseSettings` at startup; ensure `Database:ConnectionString` is configured

## Swagger / API docs
- Swagger is enabled in Development environment via `Program.cs`
- Default UI: `http://localhost:{port}/swagger` when running in Development

## Environment-specific configs
- `appsettings.Development.json` and `appsettings.Production.json` supported
- Use environment variables to override connection strings and secrets

## Recommendations
- Use `IUnitOfWork` in services to group multiple repository calls and commit once.
- Use tracked entities (fetched without `AsNoTracking`) when performing multiple updates in a single unit of work.

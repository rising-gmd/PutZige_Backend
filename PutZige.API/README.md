# PutZige.API

HTTP API for PutZige. Hosts controllers, middleware and application wiring.

## Configuration Management

The API uses environment-specific `appsettings.json` files. The configuration hierarchy is:
- `appsettings.json` (base) - contains shared defaults
- `appsettings.{Environment}.json` - environment overrides (e.g., Development, Test, QA, Staging, Production, Release)

Files present:
- `appsettings.json` - base configuration
- `appsettings.Development.json` - development overrides
- `appsettings.Test.json` - test environment overrides
- `appsettings.QA.json` - QA environment overrides
- `appsettings.Staging.json` - staging environment overrides
- `appsettings.Production.json` - production overrides
- `appsettings.Release.json` - release build overrides

How it works:
- ASP.NET Core loads `appsettings.json` first and then overlays environment-specific files based on `ASPNETCORE_ENVIRONMENT`.
- Do not commit secrets. Use User Secrets for local development and Azure Key Vault or similar for production.

Adding new configuration:
- Add the key to `appsettings.json` and optionally override in environment files.
- Use Options Pattern (`IOptions<T>`) to bind configuration to strongly typed classes.

---

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

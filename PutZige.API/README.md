# PutZige.API

REST API presentation layer for PutZige application. Implements controllers, middleware, and the HTTP pipeline. This project depends on `PutZige.Application` and `PutZige.Infrastructure`.

## Table of contents

- [Features](#features)
- [Structure](#structure)
- [Endpoints](#endpoints)
- [Authentication and Authorization](#authentication-and-authorization)
- [Configuration](#configuration)
- [Environment-specific settings](#environment-specific-settings)
- [Middleware pipeline](#middleware-pipeline)
- [Run locally](#run-locally)
- [Health checks](#health-checks)
- [Dependencies](#dependencies)
- [Related READMEs](#related-readmes)

## Features

- RESTful API design
- JWT authentication
- Swagger/OpenAPI documentation
- Global exception handling
- Request/response logging via Serilog
- CORS configuration

## Structure

```
PutZige.API/
??? Controllers/          # API endpoints
??? Middleware/           # Custom middleware
??? Filters/              # Action filters
??? Extensions/           # Extension methods for DI
??? Configuration/        # Configuration classes
??? Program.cs            # Application entry point
```

Exact paths:

- `PutZige.API/Controllers/`
- `PutZige.API/Middleware/`
- `PutZige.API/Program.cs`
- `PutZige.API/appsettings.json`

## Endpoints

### Authentication
- `POST /api/auth/register` — User registration (handler: `PutZige.Application.Services.UserService` or `PutZige.Application/UseCases/RegisterUser`)
- `POST /api/auth/login` — User login
- `POST /api/auth/refresh` — Refresh token

### Users
- `GET /api/users/{id}` — Get user profile (protected)
- `PUT /api/users/{id}` — Update user profile (protected)

If endpoints are missing in `PutZige.API/Controllers/`, mark as `[TODO]`.

## Authentication and Authorization

- JWT Bearer authentication is configured. Secrets and parameters live in `PutZige.API/appsettings.json` under `JwtSettings`.

Example `appsettings.json` snippet:

```json
{
  "JwtSettings": {
    "Secret": "[TODO_put_secret_here]",
    "Issuer": "PutZige",
    "Audience": "PutZigeClients",
    "ExpiryMinutes": 60
  }
}
```

Requests to protected endpoints require the `Authorization: Bearer {token}` header.

## Configuration

Exact config files:

- `PutZige.API/appsettings.json`
- `PutZige.API/appsettings.Development.json`
- `PutZige.API/appsettings.Production.json` (if present)

Connection string key: `ConnectionStrings:DefaultConnection`.

## Environment-specific settings

- `ASPNETCORE_ENVIRONMENT=Development` enables Swagger and developer exception pages
- Production settings should provide secrets via environment variables or a secret store

## Middleware pipeline

1. Exception handling
2. Request/response logging (Serilog)
3. CORS
4. Authentication
5. Authorization
6. Routing
7. Controllers

Adjust ordering in `PutZige.API/Program.cs` if your project shows a different pipeline.

## Developer guidance: DI and mapping

- The API leverages the Application layer to register services and cross-cutting behavior. `PutZige.Application` registers AutoMapper profiles and application services via `AddApplicationServices`.
- Controllers should accept and return DTOs defined by the Application layer and should not expose domain entities from `PutZige.Domain`.
- Validation is performed using `FluentValidation` in the Application layer; controllers perform model validation and translate errors into `ApiResponse<T>` error payloads.

## Run locally

From solution root, run:

```bash
dotnet run --project PutZige.API/PutZige.API.csproj --environment Development
```

Open Swagger UI (typical):

```
https://localhost:5001/swagger
```

## Health checks

- `GET /health` — Basic health
- `GET /health/ready` — Readiness check

## Dependencies
Check `PutZige.API/PutZige.API.csproj` for exact PackageReference entries. Common packages:

- `Swashbuckle.AspNetCore`
- `Serilog.AspNetCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`

## Related READMEs
- Root README: `../README.md`
- Application README: `../PutZige.Application/README.md`
- Infrastructure README: `../PutZige.Infrastructure/README.md`
- Domain README: `../PutZige.Domain/README.md`

## Notes
Where files or features are not present in the codebase the README uses `[TODO]` markers. Inspect the relevant folders to confirm presence.

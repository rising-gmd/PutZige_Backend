# PutZige.API

REST API presentation layer for PutZige application. Implements controllers, middleware, and the HTTP pipeline. This project depends on `PutZige.Application` and `PutZige.Infrastructure`.

## Table of contents

- [Features](#features)
- [Structure](#structure)
- [Endpoints](#endpoints)
- [Authentication and Authorization](#authentication-and-authorization)
- [Configuration](#configuration)
- [Middleware pipeline](#middleware-pipeline)
- [Run locally](#run-locally)
- [Tests](#tests)
- [Related READMEs](#related-readmes)

## Features

- RESTful API design
- JWT authentication
- Swagger/OpenAPI documentation
- Global exception handling
- Request/response logging via Serilog
- Model validation using FluentValidation

## Structure

```
PutZige.API/
  Controllers/
  Middleware/
  Filters/
  Program.cs
  appsettings.json
```

## Endpoints

User management endpoints implemented:
- `POST /api/v1/users` ? Creates a new user account (RESTful resource creation)

Authentication endpoints implemented:
- `POST /api/v1/auth/login` ? User login (returns access + refresh tokens)
- `POST /api/v1/auth/refresh-token` ? Rotate refresh token and return new access token

Messaging endpoints:
- `POST /api/v1/messages` ? Send a message to another user (requires JWT)
- `GET /api/v1/messages/conversation/{otherUserId}` ? Get conversation history with pagination (requires JWT)
- `PATCH /api/v1/messages/{messageId}/read` ? Mark message as read (requires JWT)

SignalR hub:
- `/hubs/chat` - Real-time messaging hub (JWT via query string `?access_token=`). Methods: `SendMessage(receiverId, messageText)`. Events: `ReceiveMessage`, `MessageSent`.

Controllers return `ApiResponse<T>` wrappers and validation errors use lowercase field names.

## Authentication and Authorization

JWT bearer authentication is enabled in `Program.cs` when `JwtSettings` is present in configuration. Configure `JwtSettings` in `appsettings.json` (development) or via environment variables/secrets in production.

Example `appsettings.json` snippet:
```json
{
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key-minimum-32-characters-long",
    "Issuer": "PutZige",
    "Audience": "PutZige.Users",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

## Middleware pipeline

1. GlobalExceptionHandler
2. Serilog request logging
3. HTTPS redirection
4. Authentication
5. Authorization
6. Routing

Validation filter translates ModelState to `ApiResponse<object>` using lowercase field names and appropriate error structure.

## Tests

Integration tests are in `PutZige.API.Tests` and cover registration, login, lockout and refresh-token flows.

Run integration tests:
```
dotnet test PutZige.API.Tests
```

## Run locally

From solution root:
```
dotnet run --project PutZige.API --environment Development
```

Open Swagger UI:
```
https://localhost:5001/swagger
```

## Related READMEs
- `../PutZige.Application/README.md`
- `../PutZige.Infrastructure/README.md`

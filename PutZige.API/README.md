# PutZige.API

ASP.NET Core Web API host for PutZige.

## Contains
- `Controllers/` – HTTP endpoints
- `Middleware/` – cross-cutting concerns for the pipeline

## Guidelines
- Thin controllers delegating to application layer
- Use middleware for logging, error handling, and security
- Keep API models aligned with `Application` DTOs

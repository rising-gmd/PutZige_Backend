# PutZige

Clean architecture .NET solution for the PutZige application.

## Projects
- `PutZige.Domain` – core entities, enums, and domain interfaces
- `PutZige.Application` – use cases, DTOs, validators, and application services
- `PutZige.Infrastructure` – data access, repositories, external integrations
- `PutZige.API` – ASP.NET Core Web API host

## Quick start
1. Install .NET SDK (matching the solution requirements)
2. Restore dependencies:
   - `dotnet restore`
3. Build the solution:
   - `dotnet build`
4. Run the API:
   - `dotnet run --project PutZige.API/PutZige.API.csproj`

## Next steps
- See `docs/ARCHITECTURE.md` for layer details
- See `docs/SETUP.md` for local environment setup
- See `docs/API.md` for API documentation

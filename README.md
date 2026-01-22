# PutZige

PutZige is a Whatsapp 

## Table of contents

- [Architecture](#architecture)
- [Technology stack](#technology-stack)
- [Project structure](#project-structure)
- [Quick start](#quick-start)
- [Development setup](#development-setup)
- [Build and run](#build-and-run)
- [API documentation](#api-documentation)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)
- [Related READMEs](#related-readmes)

## Architecture

Clean Architecture with DDD principles:

- **Domain Layer**: Entities, Value Objects, Domain Events
- **Application Layer**: Use Cases, DTOs, Interfaces
- **Infrastructure Layer**: Data Access, External Services implementations
- **API Layer**: REST API, Controllers, Middleware

The repository keeps business rules (domain) independent from IO and presentation concerns.

## Technology stack

- .NET 10 (projects target .NET 10)
- ASP.NET Core Web API
- Entity Framework Core (EF Core)
- AutoMapper (mapping profiles)
- MediatR (request/mediator pattern)
- FluentValidation (validators)
- Serilog (structured logging)
- Swashbuckle / Swagger (API docs)
- BCrypt.Net-Next (password hashing)

Note: exact package versions are declared in each project; see project README files below. If a version is not present in a project, it's marked `[TODO]`.

## Project structure

```
PutZige/
??? PutZige.Domain/             # Domain entities, value objects
??? PutZige.Application/        # Application services, use cases, DTOs
??? PutZige.Infrastructure/     # EF Core DbContext, repositories, services
??? PutZige.API/                # ASP.NET Core Web API project
??? *.Tests/                    # Unit and integration tests
??? README.md                   # This file
```

Physical project files (examples):

- `PutZige.Domain/PutZige.Domain.csproj`
- `PutZige.Application/PutZige.Application.csproj`
- `PutZige.Infrastructure/PutZige.Infrastructure.csproj`
- `PutZige.API/PutZige.API.csproj`

## Quick start

### Prerequisites

- .NET 10 SDK installed and on PATH
- SQL Server or PostgreSQL instance available
- (Optional) `dotnet-ef` tool for migrations

### Setup

1. Clone repository

```bash
git clone https://github.com/rising-gmd/PutZige.git
cd PutZige
```

2. Set the database connection string in `PutZige.API/appsettings.json` or via environment variable `ConnectionStrings__Default`.

Example `PutZige.API/appsettings.json` snippet:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PutZige;User Id=sa;Password=Your_password123;"
  }
}
```

3. Apply EF Core migrations (project paths are exact):

```bash
# install ef tools if needed
dotnet tool install --global dotnet-ef
# apply migrations
dotnet ef database update --project PutZige.Infrastructure/PutZige.Infrastructure.csproj --startup-project PutZige.API/PutZige.API.csproj
```

4. Run the API

```bash
dotnet run --project PutZige.API/PutZige.API.csproj --environment Development
```

## Development setup

- Recommended IDE: Visual Studio 2022/2023 or Visual Studio Code
- Restore packages: `dotnet restore`
- Build solution: `dotnet build`
- Run all tests: `dotnet test`

Environment variables supported (examples):

- `ConnectionStrings__Default` — database connection string
- `JwtSettings__Secret` — JWT signing secret
- `ASPNETCORE_ENVIRONMENT` — environment name

## Build and run

Build all projects:

```bash
dotnet build
```

Run the API locally:

```bash
dotnet run --project PutZige.API/PutZige.API.csproj
```

Run tests:

```bash
dotnet test
```

## API documentation

When running the API in Development, Swagger UI is available (typical URL):

```
https://localhost:5001/swagger
```

## Configuration

Primary configuration files (exact paths):

- `PutZige.API/appsettings.json`
- `PutZige.API/appsettings.Development.json`

For layer-specific configuration see the README files in each project directory below.

## Contributing

- Fork the repository and create feature branches: `git checkout -b feature/your-feature`
- Keep changes small and focused
- Run `dotnet test` and ensure build passes
- Open a pull request targeting `feature/*` branches per repository policy

## License

[TODO] No `LICENSE` file present in the repository root. Add a license file and update this section.

## Related READMEs

- `PutZige.Domain` - `PutZige.Domain/README.md`
- `PutZige.Application` - `PutZige.Application/README.md`
- `PutZige.Infrastructure` - `PutZige.Infrastructure/README.md`
- `PutZige.API` - `PutZige.API/README.md`

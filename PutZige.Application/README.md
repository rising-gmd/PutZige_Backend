# PutZige.Application

Application layer: use-cases, DTOs, service interfaces and validation for workflows.

## Purpose
Implements application workflows and contracts between Presentation and Domain/Infrastructure.

## Contents

### Folder structure
```
PutZige.Application/
??? Dependencies/
??? DTOs/
??? Interfaces/
??? Services/
??? Validators/
??? DependencyInjection.cs
??? README.md
```

### Key files
- `DTOs/` - request and response objects used by controllers/services
- `Interfaces/` - service interfaces (e.g., `IUserService`) describing use-cases
- `Services/` - implementations of business workflows using domain models and repositories
- `Validators/` - `FluentValidation` validators for request DTOs
- `DependencyInjection.cs` - extension method to register application services

## Dependencies
- Project reference: `PutZige.Domain`
- NuGet: `FluentValidation`
- Consumed by: `PutZige.API` during DI setup

## How services are registered
`DependencyInjection.cs` exposes an extension (e.g., `AddApplicationServices(IServiceCollection)`) that:
- Registers service implementations (`Services/*`) as scoped/transient as appropriate
- Registers validators with the service collection for controller integration

## Usage
- Controllers call into service interfaces (registered by the API)
- DTOs are mapped to domain entities inside services or via mappers in `Dependencies/`

## Design patterns
- Use-case oriented services
- Interface-driven design (service abstractions)
- Validation via FluentValidation

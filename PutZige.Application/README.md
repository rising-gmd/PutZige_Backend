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

## Design patterns
- Use-case oriented services
- Interface-driven design (service abstractions)
- Validation via FluentValidation

## Validation Pattern
We use explicit/manual validation in controllers instead of FluentValidation auto-validation. This improves testability and control over error responses.

Pattern:
1. Inject `IValidator<TRequest>` into the controller constructor.
2. In the controller action, call `await _validator.ValidateAsync(request, ct)`.
3. If `IsValid` is false convert the `ValidationResult` into a dictionary and return `BadRequest` with structured errors.

A helper `ValidationExtensions.ToDictionary()` is available in `PutZige.Application.Extensions` to convert `ValidationResult` into a `Dictionary<string, string[]>`.

Benefits:
- Explicit and testable
- Full control over error responses
- Avoids deprecated auto-validation APIs

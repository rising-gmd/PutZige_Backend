# PutZige.Application

Application layer containing business logic, use cases, DTOs, and service interfaces. This project depends on the `PutZige.Domain` layer and exposes interfaces implemented by the infrastructure layer.

## Table of contents

- [Principles](#principles)
- [Structure](#structure)
- [Patterns used](#patterns-used)
- [Key components](#key-components)
- [DTOs](#dtos)
- [Validators](#validators)
- [Mappings](#mappings)
- [Dependencies](#dependencies)
- [Usage examples](#usage-examples)
- [Related READMEs](#related-readmes)

## Principles

- Use-case driven services
- Dependency inversion: define interfaces here, implement in Infrastructure
- Separate commands and queries when beneficial
- Keep domain entities inside the Domain layer; map to DTOs for outward-facing contracts

## Structure

```
PutZige.Application/
??? UseCases/          # Application use cases (commands/queries)
??? DTOs/              # Data Transfer Objects
??? Interfaces/        # Repository and service interfaces
??? Validators/        # FluentValidation rules
??? Mappings/          # AutoMapper profiles
??? Exceptions/        # Application exceptions
```

Paths in repo (exact):

- `PutZige.Application/UseCases/`
- `PutZige.Application/DTOs/`
- `PutZige.Application/Interfaces/`
- `PutZige.Application/Validators/`
- `PutZige.Application/Mappings/`

## Patterns used

- CQRS (commands and queries separated) — [Partial/If used]
- Mediator (`MediatR`) used for request/response pipeline
- Repository interfaces (`IUserRepository`, `IRepository<T>`) defined here
- Validation using `FluentValidation`

## Key components

### Use cases (examples present)
- `RegisterUserCommand` / handler — handles user registration
- `AuthenticateUserQuery` / handler — validates credentials and returns JWT
- `GetUserProfileQuery` — reads user profile data

Files (expected):

- `PutZige.Application/UseCases/RegisterUser/RegisterUserCommand.cs`
- `PutZige.Application/UseCases/RegisterUser/RegisterUserCommandHandler.cs`

If a file is missing, see `[TODO]` tags in the codebase.

### DTOs
Common DTOs for transport between Application and Presentation:

- `UserDto` — ID, Email, IsActive, Profile fields
- `AuthResponseDto` — access token, refresh token, expiry

Files:

- `PutZige.Application/DTOs/UserDto.cs`
- `PutZige.Application/DTOs/AuthResponseDto.cs`

### Validators
Validators are implemented with `FluentValidation`. Example file paths:

- `PutZige.Application/Validators/RegisterUserValidator.cs`

## Mappings

AutoMapper profiles live in the `Mappings` folder and follow a one-profile-per-aggregate convention. Profiles are responsible for mapping domain entities to DTOs used across the API surface.

- Location: `PutZige.Application/Mappings/`
- Example profile: `UserMappingProfile.cs` — maps `PutZige.Domain.Entities.User` ? `PutZige.Application.DTOs.Auth.RegisterUserResponse` (maps `Id` ? `UserId`).
- Registration: AutoMapper is registered in DI by the Application layer (see `PutZige.Application/DependencyInjection.cs`) using `services.AddAutoMapper(Assembly.GetExecutingAssembly())` so profiles are discovered automatically.
- Best practices:
  - Inject `IMapper` into services; avoid static `Mapper.Map()` usage.
  - Prefer mapping to DTOs at the boundary of the application layer; services should return DTOs (not domain entities) for presentation layers.
  - Add mapping unit tests using `MapperConfiguration.AssertConfigurationIsValid()`.

## Dependencies
NuGet packages referenced by this project (check `PutZige.Application/PutZige.Application.csproj` for exact versions):

- `MediatR`
- `FluentValidation`
- `AutoMapper`
- `AutoMapper.Extensions.Microsoft.DependencyInjection`

Exact PackageReference entries are present in the project file; package versions are centrally managed in `Directory.Packages.props`.

## Usage examples

Command and handler example (simplified):

```csharp
public class RegisterUserCommand : IRequest<UserDto>
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// Handler resolves IUserRepository and password hasher via DI
```

Validation example:

```csharp
// PutZige.Application/Validators/RegisterUserValidator.cs
RuleFor(x => x.Email).NotEmpty().EmailAddress();
RuleFor(x => x.Password).MinimumLength(8);
```

Mapping example (service):

```csharp
// Injected IMapper in a service
var response = _mapper.Map<RegisterUserResponse>(userEntity);
```

Service contract guidance:

- Services in this layer expose DTOs (e.g. `RegisterUserResponse`) to callers; do not return domain entities from service public methods.

## Related READMEs
- Root README: `../README.md`
- Domain README: `../PutZige.Domain/README.md`
- Infrastructure README: `../PutZige.Infrastructure/README.md`
- API README: `../PutZige.API/README.md`

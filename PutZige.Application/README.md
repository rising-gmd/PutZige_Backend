# PutZige.Application

Application layer containing business logic, use cases, DTOs, and service interfaces. This project depends on the `PutZige.Domain` layer and exposes interfaces implemented by the infrastructure layer.

## Table of contents

- [Principles](#principles)
- [Structure](#structure)
- [Patterns used](#patterns-used)
- [Key components](#key-components)
- [DTOs](#dtos)
- [Validators](#validators)
- [Dependencies](#dependencies)
- [Usage examples](#usage-examples)
- [Related READMEs](#related-readmes)

## Principles

- Use-case driven services
- Dependency inversion: define interfaces here, implement in Infrastructure
- Separate commands and queries when beneficial

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

## Dependencies
NuGet packages referenced by this project (check `PutZige.Application/PutZige.Application.csproj` for exact versions):

- `MediatR`
- `FluentValidation`
- `AutoMapper`

Exact PackageReference entries are present in the project file; if not present mark as `[TODO]`.

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

## Related READMEs
- Root README: `../README.md`
- Domain README: `../PutZige.Domain/README.md`
- Infrastructure README: `../PutZige.Infrastructure/README.md`
- API README: `../PutZige.API/README.md`

# Validators

## Overview
This folder contains FluentValidation validators used to automatically validate incoming API requests. Validators are registered with DI and will be invoked by ASP.NET Core model validation when `AddFluentValidationAutoValidation()` is enabled.

## Rules
- `RegisterUserRequestValidator` validates the registration payload ensuring email/username/displayName/password meet the security and formatting constraints.

## Auto-validation
- ASP.NET Core will surface validation errors as ModelState entries and the GlobalExceptionHandlerMiddleware maps FluentValidation.ValidationException to a 400 response when thrown manually.

## Extending
- Add new `AbstractValidator<T>` implementations for new request DTOs. Registering is automatic via `services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())`.

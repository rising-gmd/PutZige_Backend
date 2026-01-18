# Application/Common

This folder contains centralized constants and messages used across the application.

## Constants (AppConstants)

Location: `PutZige.Application.Common.Constants.AppConstants`

Purpose:
- Centralize application compile-time constants such as validation limits, regex patterns, and default timeouts.
- Improve maintainability and reduce magic numbers/strings.
- Group related values into nested static classes (Validation, Security, Http, Database).

Usage:
- Reference `AppConstants.Validation.MinUsernameLength` in validators.
- Reference `AppConstants.Security.BcryptWorkFactor` in password hashing.

## Messages

Files:
- `ErrorMessages` - user-facing error strings
- `SuccessMessages` - user-facing success strings
- `LogMessages` - structured log message templates with placeholders

Benefits:
- Consistent user messages across controllers and services.
- Easier localization in future by replacing these classes with resource files.
- Central place to review and edit wording.

Before/After example:

Before:
- `throw new InvalidOperationException("Email already taken");`
- `_logger.LogInformation("User registered successfully. UserId: {0}", userId);`
- `return Ok(new { message = "Registration successful" });`

After:
- `throw new InvalidOperationException(ErrorMessages.Authentication.EmailAlreadyTaken);`
- `_logger.LogInformation(LogMessages.Authentication.RegistrationSuccessful, userId, email);`
- `return Ok(ApiResponse.Success(data, SuccessMessages.Authentication.RegistrationSuccessful));`

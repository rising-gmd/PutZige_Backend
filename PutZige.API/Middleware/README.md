# Middleware

## Pipeline order
1. `GlobalExceptionHandlerMiddleware` - must run first to capture all exceptions
2. `Routing`
3. `Authentication` (future)
4. `Authorization` (future)

## GlobalExceptionHandlerMiddleware
- Maps exceptions to status codes:
  - `InvalidOperationException` -> 400
  - `KeyNotFoundException` -> 404
  - `UnauthorizedAccessException` -> 401
  - `ArgumentException` / `ArgumentNullException` -> 400
  - `FluentValidation.ValidationException` -> 400
  - default -> 500
- Returns an `ApiResponse<object>.Error(...)` JSON payload
- Logs 5xx as Error and 4xx as Warning

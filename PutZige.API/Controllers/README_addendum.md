## Validation Pattern (Manual Validation)

Controllers use manual validation with `FluentValidation` validators injected via `IValidator<T>`.

Pattern:
- Inject `IValidator<TRequest>` into the controller constructor.
- Call `await _validator.ValidateAsync(request, ct)` at the start of the action.
- If `IsValid` is false use `ValidationExtensions.ToDictionary()` to get a property->errors map and return `BadRequest` with structured errors.

Why manual validation?
- Explicit control over validation flow and error formatting.
- Better for unit testing controllers and error handling logic.
- Avoids relying on the deprecated auto-validation integration.

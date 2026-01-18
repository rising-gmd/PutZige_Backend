# Application Extensions

## ValidationExtensions

`ValidationExtensions.ToDictionary()` converts a `FluentValidation.Results.ValidationResult` into a `Dictionary<string, string[]>` keyed by property name with an array of error messages per property.

Usage example:

```csharp
using PutZige.Application.Extensions;
using FluentValidation.Results;

ValidationResult result = await _validator.ValidateAsync(request, ct);
if (!result.IsValid)
{
    var errors = result.ToDictionary();
    return BadRequest(ApiResponse<object>.Error("Validation failed", errors, 400));
}
```

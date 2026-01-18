#nullable enable
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace PutZige.Application.Extensions;

public static class ValidationExtensions
{
    public static Dictionary<string, string[]> ToDictionary(this ValidationResult? validationResult)
    {
        if (validationResult == null || validationResult.Errors == null) return new Dictionary<string, string[]>();

        return validationResult.Errors
            .Where(e => !string.IsNullOrWhiteSpace(e.PropertyName))
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage ?? string.Empty).ToArray()
            );
    }
}

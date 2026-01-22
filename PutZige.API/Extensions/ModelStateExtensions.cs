using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PutZige.API.Extensions
{
    public static class ModelStateExtensions
    {
        public static string ToCamelCaseField(this string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return key ?? string.Empty;

            var lastSegment = key.Contains('.') ? key[(key.LastIndexOf('.') + 1)..] : key;
            if (string.IsNullOrEmpty(lastSegment))
                return lastSegment;

            if (lastSegment.Length == 1)
                return lastSegment.ToLowerInvariant();

            return char.ToLowerInvariant(lastSegment[0]) + lastSegment[1..];
        }

        public static Dictionary<string, string[]> ToErrorsDictionary(this ModelStateDictionary modelState)
        {
            if (modelState == null) throw new ArgumentNullException(nameof(modelState));

            return modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key.ToCamelCaseField(),
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
        }
    }
}

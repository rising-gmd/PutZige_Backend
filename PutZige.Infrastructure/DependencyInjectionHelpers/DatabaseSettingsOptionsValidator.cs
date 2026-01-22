#nullable enable
using Microsoft.Extensions.Options;
using PutZige.Infrastructure.Settings;
using System;

namespace PutZige.Infrastructure.DependencyInjectionHelpers;

internal class DatabaseSettingsOptionsValidator : IValidateOptions<DatabaseSettings>
{
    public ValidateOptionsResult Validate(string? name, DatabaseSettings options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("DatabaseSettings is not configured.");

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            return ValidateOptionsResult.Fail("ConnectionString must be provided.");

        if (options.CommandTimeout <= 0)
            return ValidateOptionsResult.Fail("CommandTimeout must be greater than zero.");

        if (options.MaxRetryCount < 0)
            return ValidateOptionsResult.Fail("MaxRetryCount must be non-negative.");

        return ValidateOptionsResult.Success;
    }
}

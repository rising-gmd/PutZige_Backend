#nullable enable
using Microsoft.Extensions.Options;
using PutZige.Application.Settings;

namespace PutZige.Infrastructure.DependencyInjectionHelpers;

internal class HashingSettingsOptionsValidator : IValidateOptions<HashingSettings>
{
    public ValidateOptionsResult Validate(string? name, HashingSettings options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("HashingSettings is not configured.");

        if (options.SaltSizeBytes < 16 || options.SaltSizeBytes > 64)
            return ValidateOptionsResult.Fail("SaltSizeBytes must be between 16 and 64.");

        if (options.Algorithm != "SHA256" && options.Algorithm != "SHA512")
            return ValidateOptionsResult.Fail("Algorithm must be SHA256 or SHA512.");

        if (options.Iterations < 10000 || options.Iterations > 1000000)
            return ValidateOptionsResult.Fail("Iterations must be between 10000 and 1000000.");

        return ValidateOptionsResult.Success;
    }
}

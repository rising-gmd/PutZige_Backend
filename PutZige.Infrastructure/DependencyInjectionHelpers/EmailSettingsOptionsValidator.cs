using Microsoft.Extensions.Options;

namespace PutZige.Infrastructure.DependencyInjectionHelpers;

internal class EmailSettingsOptionsValidator : IValidateOptions<PutZige.Infrastructure.Settings.EmailSettings>
{
    public ValidateOptionsResult Validate(string? name, PutZige.Infrastructure.Settings.EmailSettings options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("EmailSettings is not configured.");

        if (string.IsNullOrWhiteSpace(options.SmtpHost))
            return ValidateOptionsResult.Fail("SmtpHost must be provided.");

        if (options.SmtpPort <= 0 || options.SmtpPort > 65535)
            return ValidateOptionsResult.Fail("SmtpPort must be a valid port number.");

        if (string.IsNullOrWhiteSpace(options.FromEmail))
            return ValidateOptionsResult.Fail("FromEmail must be provided.");

        if (string.IsNullOrWhiteSpace(options.VerificationLinkBaseUrl))
            return ValidateOptionsResult.Fail("VerificationLinkBaseUrl must be provided.");

        return ValidateOptionsResult.Success;
    }
}

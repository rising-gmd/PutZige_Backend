using FluentValidation;

namespace PutZige.Infrastructure.Settings;

public sealed class EmailSettingsValidator : AbstractValidator<EmailSettings>
{
    public EmailSettingsValidator()
    {
        RuleFor(x => x.SmtpHost).NotEmpty().WithMessage("SmtpHost must be provided in configuration.");
        RuleFor(x => x.SmtpPort).GreaterThan(0).WithMessage("SmtpPort must be greater than 0.");
        RuleFor(x => x.FromEmail).NotEmpty().WithMessage("FromEmail must be provided in configuration.");
        RuleFor(x => x.VerificationLinkBaseUrl).NotEmpty().WithMessage("VerificationLinkBaseUrl must be provided in configuration.");
    }
}

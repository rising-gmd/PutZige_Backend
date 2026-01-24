using FluentValidation;
using PutZige.Application.Settings;

namespace PutZige.Application.Validators
{
    public class HashingSettingsValidator : AbstractValidator<HashingSettings>
    {
        public HashingSettingsValidator()
        {
            RuleFor(x => x.SaltSizeBytes).InclusiveBetween(16, 64);
            RuleFor(x => x.Algorithm).Must(a => a == "SHA256" || a == "SHA512").WithMessage("Algorithm must be SHA256 or SHA512");
            RuleFor(x => x.Iterations).InclusiveBetween(10000, 1000000);
        }
    }
}
using FluentValidation;
using PutZige.Infrastructure.Settings;

namespace PutZige.Infrastructure.Settings;

public sealed class DatabaseSettingsValidator : AbstractValidator<DatabaseSettings>
{
    public DatabaseSettingsValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("ConnectionString must be provided in configuration.");

        RuleFor(x => x.MaxRetryCount)
            .GreaterThanOrEqualTo(0).WithMessage("MaxRetryCount must be >= 0.");

        RuleFor(x => x.CommandTimeout)
            .GreaterThan(0).WithMessage("CommandTimeout must be greater than 0.");
    }
}
// File: src/PutZige.Infrastructure/Configuration/DatabaseSettingsValidator.cs

using FluentValidation;

namespace PutZige.Infrastructure.Configuration;

public class DatabaseSettingsValidator : AbstractValidator<DatabaseSettings>
{
    public DatabaseSettingsValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("ConnectionString is required.")
            .Must(cs => cs.Contains("Database=")).WithMessage("ConnectionString must contain 'Database='.");

        RuleFor(x => x.MaxRetryCount)
            .InclusiveBetween(1, 10).WithMessage("MaxRetryCount must be between 1 and 10.");

        RuleFor(x => x.CommandTimeout)
            .InclusiveBetween(10, 300).WithMessage("CommandTimeout must be between 10 and 300 seconds.");
    }
}

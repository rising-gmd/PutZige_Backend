#nullable enable
using FluentValidation;
using PutZige.Application.DTOs.Auth;

namespace PutZige.Application.Validators.Auth
{
    /// <summary>
    /// Validation rules for RegisterUserRequest.
    /// </summary>
    public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.Username)
                .NotEmpty()
                .Length(3, 30)
                .Matches("^[a-zA-Z0-9_]+$")
                .WithMessage("Username must be alphanumeric with optional underscores and between 3 and 30 characters.");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .Length(2, 50);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches(@"[!@#$%^&*()_+\-=[\]{}|;:,.<>?]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}

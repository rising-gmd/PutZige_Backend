#nullable enable
using FluentValidation;
using PutZige.Application.DTOs.Auth;
using PutZige.Application.Common.Constants;
using PutZige.Application.Common.Messages;

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
                .NotEmpty().WithMessage(ErrorMessages.Validation.EmailRequired)
                .EmailAddress().WithMessage(ErrorMessages.Validation.EmailInvalidFormat)
                .MaximumLength(AppConstants.Validation.MaxEmailLength).WithMessage(ErrorMessages.Validation.EmailTooLong)
                .WithName("email");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(ErrorMessages.Validation.UsernameRequired)
                .Length(AppConstants.Validation.MinUsernameLength, AppConstants.Validation.MaxUsernameLength).WithMessage(ErrorMessages.Validation.UsernameInvalidLength)
                .Matches(AppConstants.Validation.UsernameRegexPattern).WithMessage(ErrorMessages.Validation.UsernameInvalidCharacters)
                .WithName("username");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage(ErrorMessages.Validation.DisplayNameRequired)
                .Length(AppConstants.Validation.MinDisplayNameLength, AppConstants.Validation.MaxDisplayNameLength).WithMessage(ErrorMessages.Validation.DisplayNameInvalidLength)
                .WithName("displayName");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ErrorMessages.Validation.PasswordRequired)
                .MinimumLength(AppConstants.Validation.MinPasswordLength).WithMessage(ErrorMessages.Validation.PasswordTooShort)
                .Matches(AppConstants.Validation.PasswordUppercaseRegex).WithMessage(ErrorMessages.Validation.PasswordMissingUppercase)
                .Matches(AppConstants.Validation.PasswordLowercaseRegex).WithMessage(ErrorMessages.Validation.PasswordMissingLowercase)
                .Matches(AppConstants.Validation.PasswordDigitRegex).WithMessage(ErrorMessages.Validation.PasswordMissingDigit)
                .Matches(AppConstants.Validation.PasswordSpecialCharRegex).WithMessage(ErrorMessages.Validation.PasswordMissingSpecialChar)
                .WithName("password");
        }
    }
}

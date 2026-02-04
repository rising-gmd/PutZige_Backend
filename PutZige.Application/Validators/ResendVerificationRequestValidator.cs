using FluentValidation;
using PutZige.Application.DTOs.Auth;

namespace PutZige.Application.Validators;

public sealed class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
{
    public ResendVerificationRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithName("email").WithMessage(PutZige.Application.Common.Messages.ErrorMessages.Validation.EmailRequired)
                .EmailAddress().WithMessage(PutZige.Application.Common.Messages.ErrorMessages.Validation.EmailInvalidFormat);
        }
}

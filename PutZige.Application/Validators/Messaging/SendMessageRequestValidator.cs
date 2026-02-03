using FluentValidation;
using PutZige.Application.Common.Constants;
using PutZige.Application.DTOs.Messaging;

namespace PutZige.Application.Validators.Messaging;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.ReceiverId).NotEmpty().WithName("receiverId").WithMessage(PutZige.Application.Common.Messages.ErrorMessages.Messaging.ReceiverIdRequired);
        RuleFor(x => x.MessageText)
            .NotEmpty().WithName("messageText").WithMessage(PutZige.Application.Common.Messages.ErrorMessages.Messaging.MessageTextRequired)
            .MaximumLength(AppConstants.Messaging.MaxMessageLength).WithMessage(PutZige.Application.Common.Messages.ErrorMessages.Messaging.MessageTooLong);
    }
}

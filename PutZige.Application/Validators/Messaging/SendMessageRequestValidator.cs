using FluentValidation;
using PutZige.Application.Common.Constants;
using PutZige.Application.DTOs.Messaging;

namespace PutZige.Application.Validators.Messaging;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.ReceiverId).NotEmpty().WithName("receiverId");
        RuleFor(x => x.MessageText).NotEmpty().WithName("messageText").MaximumLength(AppConstants.Messaging.MaxMessageLength);
    }
}

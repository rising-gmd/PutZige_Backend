using FluentValidation;
using PutZige.Application.Common.Constants;
using PutZige.Application.DTOs.Messaging;

namespace PutZige.Application.Validators.Messaging;

public class ConversationHistoryRequestValidator : AbstractValidator<ConversationHistoryRequest>
{
    public ConversationHistoryRequestValidator()
    {
        RuleFor(x => x.OtherUserId).NotEmpty().WithName("otherUserId");
        RuleFor(x => x.PageNumber).GreaterThan(0).WithName("pageNumber");
        RuleFor(x => x.PageSize).InclusiveBetween(1, AppConstants.Messaging.MaxPageSize).WithName("pageSize");
    }
}

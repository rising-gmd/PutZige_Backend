using System;

namespace PutZige.Application.DTOs.Messaging;

public record ConversationHistoryRequest
{
    public Guid OtherUserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

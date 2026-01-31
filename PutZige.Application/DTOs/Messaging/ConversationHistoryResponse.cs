using System.Collections.Generic;

namespace PutZige.Application.DTOs.Messaging;

public record ConversationHistoryResponse
{
    public List<MessageDto> Messages { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public bool HasNextPage => (PageNumber * PageSize) < TotalCount;
}

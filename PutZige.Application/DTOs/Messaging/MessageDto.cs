using System;

namespace PutZige.Application.DTOs.Messaging;

public record MessageDto
{
    public Guid Id { get; init; }
    public Guid SenderId { get; init; }
    public string SenderUsername { get; init; } = string.Empty;
    public Guid ReceiverId { get; init; }
    public string ReceiverUsername { get; init; } = string.Empty;
    public string MessageText { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public bool IsRead => ReadAt.HasValue;
}

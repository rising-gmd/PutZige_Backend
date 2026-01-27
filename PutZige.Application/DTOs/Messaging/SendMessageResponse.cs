using System;

namespace PutZige.Application.DTOs.Messaging;

public record SendMessageResponse(Guid MessageId, Guid SenderId, Guid ReceiverId, string MessageText, DateTime SentAt);

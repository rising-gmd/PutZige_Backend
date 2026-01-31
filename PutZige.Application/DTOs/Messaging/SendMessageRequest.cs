using System;

namespace PutZige.Application.DTOs.Messaging;

public record SendMessageRequest(Guid ReceiverId, string MessageText);

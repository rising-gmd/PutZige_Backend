#nullable enable
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PutZige.Application.Interfaces;
using PutZige.Application.DTOs.Messaging;

namespace PutZige.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, string> UserConnections = new();
    private readonly IMessagingService _messagingService;
    private readonly ILogger<ChatHub>? _logger;

    public ChatHub(IMessagingService messagingService, ILogger<ChatHub>? logger = null)
    {
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromClaims();
        if (userId != Guid.Empty)
        {
            UserConnections[userId] = Context.ConnectionId;
            _logger?.LogInformation("User connected - UserId: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserIdFromClaims();
        if (userId != Guid.Empty)
        {
            UserConnections.TryRemove(userId, out _);
            _logger?.LogInformation("User disconnected - UserId: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid receiverId, string messageText)
    {
        var senderId = GetUserIdFromClaims();
        var response = await _messagingService.SendMessageAsync(senderId, receiverId, messageText);

        if (UserConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", response);
            await _messagingService.MarkMessageAsDeliveredAsync(response.MessageId);
        }

        await Clients.Caller.SendAsync("MessageSent", response);
    }

    private Guid GetUserIdFromClaims()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(claim, out var id)) return id;
        return Guid.Empty;
    }
}

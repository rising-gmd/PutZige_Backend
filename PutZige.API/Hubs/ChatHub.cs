#nullable enable
using System;
using System.Collections.Concurrent;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ChatHub>? _logger;

    public ChatHub(IMessagingService messagingService, ICurrentUserService currentUserService, ILogger<ChatHub>? logger = null)
    {
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = _currentUserService.GetUserId();

            UserConnections[userId] = Context.ConnectionId;

            _logger?.LogInformation("User connected - UserId: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to establish connection for caller {ConnectionId}; aborting", Context.ConnectionId);

            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = _currentUserService.TryGetUserId();

            if (userId.HasValue)
            {
                UserConnections.TryRemove(userId.Value, out _);

                _logger?.LogInformation("User disconnected - UserId: {UserId}, ConnectionId: {ConnectionId}", userId.Value, Context.ConnectionId);
            }

        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error while handling disconnect for ConnectionId: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid receiverId, string messageText)
    {
        try
        {
            var senderId = _currentUserService.GetUserId();

            var response = await _messagingService.SendMessageAsync(senderId, receiverId, messageText);

            if (UserConnections.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", response);

                await _messagingService.MarkMessageAsDeliveredAsync(response.MessageId);
            }

            await Clients.Caller.SendAsync("MessageSent", response);
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogWarning(ex, "Unauthenticated user attempted to send a message from ConnectionId: {ConnectionId}", Context.ConnectionId);

            await Clients.Caller.SendAsync("Error", ex.Message);

            Context.Abort();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send message from ConnectionId: {ConnectionId}", Context.ConnectionId);

            throw;
        }
    }

}

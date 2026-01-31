#nullable enable
using System;
using System.Collections.Concurrent;
using PutZige.Application.Interfaces;

namespace PutZige.Infrastructure.Services;

public sealed class ConnectionMappingService : IConnectionMappingService
{
    private readonly ConcurrentDictionary<Guid, string> _connections = new();

    public void Add(Guid userId, string connectionId)
    {
        _connections[userId] = connectionId;
    }

    public bool Remove(Guid userId)
    {
        return _connections.TryRemove(userId, out _);
    }

    public bool TryGetConnection(Guid userId, out string? connectionId)
    {
        return _connections.TryGetValue(userId, out connectionId);
    }

    public void Clear()
    {
        _connections.Clear();
    }
}

#nullable enable
using System;

namespace PutZige.Application.Interfaces;

public interface IConnectionMappingService
{
    void Add(Guid userId, string connectionId);
    bool Remove(Guid userId);
    bool TryGetConnection(Guid userId, out string? connectionId);
    void Clear(); // For testing only
}

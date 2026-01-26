using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PutZige.Domain.Entities;

namespace PutZige.Domain.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IEnumerable<Message> Messages, int TotalCount)> GetConversationAsync(Guid userId, Guid otherUserId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task AddAsync(Message message, CancellationToken ct = default);
    Task UpdateAsync(Message message, CancellationToken ct = default);
}

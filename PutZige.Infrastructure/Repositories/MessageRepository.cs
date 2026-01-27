#nullable enable
using Microsoft.EntityFrameworkCore;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Infrastructure.Repositories;

public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context) : base(context) { }

    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.Include(m => m.Sender).Include(m => m.Receiver).FirstOrDefaultAsync(m => m.Id == id, ct).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Message> Messages, int TotalCount)> GetConversationAsync(Guid userId, Guid otherUserId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = _dbSet.Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || (m.SenderId == otherUserId && m.ReceiverId == userId))
                          .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync(ct).ConfigureAwait(false);

        var skip = (pageNumber - 1) * pageSize;

        var messages = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(m => new Message
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                MessageText = m.MessageText,
                SentAt = m.SentAt,
                DeliveredAt = m.DeliveredAt,
                ReadAt = m.ReadAt,
                CreatedAt = m.CreatedAt,
                Sender = new User { Id = m.SenderId, Username = m.Sender != null ? m.Sender.Username : string.Empty },
                Receiver = new User { Id = m.ReceiverId, Username = m.Receiver != null ? m.Receiver.Username : string.Empty }
            })
            .ToListAsync(ct).ConfigureAwait(false);

        return (messages, totalCount);
    }

    public async Task AddAsync(Message message, CancellationToken ct = default)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        await _dbSet.AddAsync(message, ct).ConfigureAwait(false);
    }

    public Task UpdateAsync(Message message, CancellationToken ct = default)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        _dbSet.Update(message);
        return Task.CompletedTask;
    }
}

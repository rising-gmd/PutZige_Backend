#nullable enable
using Microsoft.EntityFrameworkCore;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for entities inheriting from <see cref="BaseEntity"/>.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    public Repository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet.AsQueryable();
        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(e => e.Id == id, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(CancellationToken ct = default)
        => await _dbSet.CountAsync(ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.CountAsync(predicate, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        await _dbSet.AddAsync(entity, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual void Update(TEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Update(entity);
    }

    /// <inheritdoc/>
    public virtual void Delete(TEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    /// <inheritdoc/>
    public virtual void HardDelete(TEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Remove(entity);
    }
}

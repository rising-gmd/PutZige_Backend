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

namespace PutZige.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation for entities inheriting from BaseEntity.
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
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        /// <inheritdoc />
        public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);

        /// <inheritdoc />
        public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);
            return await query.FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default)
            => await _dbSet.ToListAsync(ct);

        /// <inheritdoc />
        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => await _dbSet.FirstOrDefaultAsync(predicate, ct);

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => await _dbSet.Where(predicate).ToListAsync(ct);

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(CancellationToken ct = default)
            => await _dbSet.CountAsync(ct);

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => await _dbSet.CountAsync(predicate, ct);

        /// <inheritdoc />
        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => await _dbSet.AnyAsync(predicate, ct);

        /// <inheritdoc />
        public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
            => await _dbSet.AddAsync(entity, ct);

        /// <inheritdoc />
        public virtual void Update(TEntity entity)
            => _dbSet.Update(entity);

        /// <inheritdoc />
        public virtual void Delete(TEntity entity)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        /// <inheritdoc />
        public virtual void HardDelete(TEntity entity)
            => _dbSet.Remove(entity);
    }
}

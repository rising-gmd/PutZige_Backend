// File: src/PutZige.Domain/Interfaces/IRepository.cs
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface for basic CRUD and query operations.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets an entity by its primary key.
        /// </summary>
        Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default);

        /// <summary>
        /// Gets an entity by its primary key with related entities included.
        /// </summary>
        Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets the first entity matching the predicate or null.
        /// </summary>
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Finds all entities matching the predicate.
        /// </summary>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Counts all entities.
        /// </summary>
        Task<int> CountAsync(CancellationToken ct = default);

        /// <summary>
        /// Counts entities matching the predicate.
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Checks if any entity matches the predicate.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        Task AddAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Marks an entity as modified.
        /// </summary>
        void Update(TEntity entity);

        /// <summary>
        /// Marks an entity as deleted.
        /// </summary>
        void Delete(TEntity entity);
    }
}

// File: src/PutZige.Infrastructure/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Infrastructure.Repositories
{
    /// <summary>
    /// User repository implementation.
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        /// <inheritdoc />
        public virtual async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        /// <inheritdoc />
        public virtual async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, ct);
        }

        /// <inheritdoc />
        public virtual async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().AnyAsync(u => u.Email == email, ct);
        }

        /// <inheritdoc />
        public virtual async Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().AnyAsync(u => u.Username == username, ct);
        }
    }
}

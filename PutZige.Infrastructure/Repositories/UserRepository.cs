using Microsoft.EntityFrameworkCore;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Data;
using System;
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
        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _dbSet.FirstOrDefaultAsync(u => u.Email == email, ct);

        /// <inheritdoc />
        public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
            => await _dbSet.FirstOrDefaultAsync(u => u.Username == username, ct);

        /// <inheritdoc />
        public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
            => await _dbSet.AnyAsync(u => u.Email == email, ct);

        /// <inheritdoc />
        public async Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default)
            => await _dbSet.AnyAsync(u => u.Username == username, ct);
    }
}

#nullable enable
using Microsoft.EntityFrameworkCore;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Infrastructure.Repositories;

/// <summary>
/// User repository implementation.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserRepository"/>.
    /// </summary>
    public UserRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email is required", nameof(email));
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username is required", nameof(username));
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return await _dbSet.AsNoTracking().AnyAsync(u => u.Email == email, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return await _dbSet.AsNoTracking().AnyAsync(u => u.Username == username, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByEmailWithSessionAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email is required", nameof(email));
        return await _dbSet.Include(u => u.Session).FirstOrDefaultAsync(u => u.Email == email, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByUsernameWithSessionAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username is required", nameof(username));
        return await _dbSet.Include(u => u.Session).FirstOrDefaultAsync(u => u.Username == username, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;
        // We cannot directly compare plaintext refreshToken to stored hash. Return user with session if session exists; calling code must verify token with hashing service.
        return await _dbSet.Include(u => u.Session).FirstOrDefaultAsync(u => u.Session != null && u.Session.RefreshTokenHash != null, ct).ConfigureAwait(false);
    }
}

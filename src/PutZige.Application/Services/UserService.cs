// File: src/PutZige.Application/Services/UserService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Data;

namespace PutZige.Application.Services
{
    /// <summary>
    /// Service for user registration and retrieval.
    /// </summary>
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;

        public UserService(IUserRepository userRepository, AppDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        /// <summary>
        /// Registers a new user with validation and password hashing.
        /// </summary>
        public async Task<User> RegisterUserAsync(
            string email,
            string username,
            string displayName,
            string password,
            CancellationToken ct = default)
        {
            // 1. Validate email not taken
            if (await _userRepository.IsEmailTakenAsync(email, ct))
                throw new InvalidOperationException("Email is already taken.");

            // 2. Validate username not taken
            if (await _userRepository.IsUsernameTakenAsync(username, ct))
                throw new InvalidOperationException("Username is already taken.");

            // 3. Hash password using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // 4. Create User entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = username,
                DisplayName = displayName,
                PasswordHash = passwordHash,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                // 5. Generate email verification token
                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            // 6. Add to repository
            await _userRepository.AddAsync(user, ct);

            // 7. Save changes
            await _context.SaveChangesAsync(ct);

            // 8. Return created user
            return user;
        }

        /// <summary>
        /// Gets a user by email.
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _userRepository.GetByEmailAsync(email, ct);
        }
    }
}

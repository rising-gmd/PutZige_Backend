#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using PutZige.Application.Interfaces;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using BCrypt.Net;
using System.Security.Cryptography;

namespace PutZige.Application.Services
{
    /// <summary>
    /// Service for user registration and management.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(unitOfWork);

            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Registers a new user with validation and hashing.
        /// </summary>
        public async Task<User> RegisterUserAsync(
            string email,
            string username,
            string displayName,
            string password,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required", nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required", nameof(password));

            // Check availability
            if (await _userRepository.IsEmailTakenAsync(email, ct))
                throw new InvalidOperationException("Email already taken");

            if (await _userRepository.IsUsernameTakenAsync(username, ct))
                throw new InvalidOperationException("Username already taken");

            // Create a cryptographically secure verification token
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');

            // Use a reasonable work factor; consider making this configurable
            const int bcryptWorkFactor = 12;

            var user = new User
            {
                Email = email,
                Username = username,
                DisplayName = displayName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, bcryptWorkFactor),
                EmailVerificationToken = token,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(1),
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // TODO: Send verification email to {user.Email} with token {user.EmailVerificationToken}

            return user;
        }

        /// <summary>
        /// Gets a user by email.
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
            => await _userRepository.GetByEmailAsync(email, ct);

        /// <summary>
        /// Soft deletes a user by id.
        /// </summary>
        public async Task SoftDeleteUserAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null) throw new KeyNotFoundException("User not found");
            _userRepository.Delete(user);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}

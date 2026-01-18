#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using PutZige.Application.Interfaces;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using BCrypt.Net;

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
            if (await _userRepository.IsEmailTakenAsync(email, ct))
                throw new InvalidOperationException("Email already taken");
            if (await _userRepository.IsUsernameTakenAsync(username, ct))
                throw new InvalidOperationException("Username already taken");

            var user = new User
            {
                Email = email,
                Username = username,
                DisplayName = displayName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(1)
            };
            await _userRepository.AddAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);
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

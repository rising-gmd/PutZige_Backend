#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using PutZige.Application.Interfaces;
using PutZige.Application.DTOs.Auth;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using System.Security.Cryptography;
using PutZige.Application.Common.Constants;
using PutZige.Application.Common.Messages;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace PutZige.Application.Services
{
    /// <summary>
    /// Service for user registration and management.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService>? _logger;
        private readonly IMapper _mapper;
        private readonly IHashingService _hashingService;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper, IHashingService hashingService, ILogger<UserService>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(hashingService);

            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _hashingService = hashingService;
        }

        /// <summary>
        /// Registers a new user with validation and hashing and returns a response DTO.
        /// </summary>
        public async Task<RegisterUserResponse> RegisterUserAsync(string email, string username, string displayName, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(ErrorMessages.Validation.EmailRequired, nameof(email));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException(ErrorMessages.Validation.UsernameRequired, nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException(ErrorMessages.Validation.PasswordRequired, nameof(password));

            _logger?.LogInformation("User registration attempt - Email: {Email}", email);

            // Check availability
            if (await _userRepository.IsEmailTakenAsync(email, ct))
            {
                _logger?.LogWarning("Registration failed - Email already exists: {Email}", email);
                throw new InvalidOperationException(ErrorMessages.Authentication.EmailAlreadyTaken);
            }

            if (await _userRepository.IsUsernameTakenAsync(username, ct))
            {
                _logger?.LogWarning("Registration failed - Username already exists: {Username}", username);
                throw new InvalidOperationException(ErrorMessages.Authentication.UsernameAlreadyTaken);
            }

            // Create a cryptographically secure verification token
            var token = _hashingService.GenerateSecureToken(32);

            // Hash password
            var hashed = await _hashingService.HashAsync(password, ct);

            var user = new User
            {
                Email = email,
                Username = username,
                DisplayName = displayName,
                PasswordHash = hashed.Hash,
                PasswordSalt = hashed.Salt,
                EmailVerificationToken = token,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(AppConstants.Security.EmailVerificationTokenExpirationDays),
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            _logger?.LogInformation("Creating user entity - Email: {Email}", email);

            await _userRepository.AddAsync(user, ct);

            _logger?.LogInformation("Saving changes to database");

            await _unitOfWork.SaveChangesAsync(ct);

            _logger?.LogInformation("User entity created - UserId: {UserId}", user.Id);

            // TODO: Send verification email to {user.Email} with token {user.EmailVerificationToken}

            // Map to response DTO
            var response = _mapper.Map<RegisterUserResponse>(user);

            return response;
        }

        /// <summary>
        /// Gets a user by email.
        /// </summary>
        public async Task<RegisterUserResponse?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, ct);
            return user == null ? null : _mapper.Map<RegisterUserResponse>(user);
        }

        /// <summary>
        /// Soft deletes a user by id.
        /// </summary>
        public async Task SoftDeleteUserAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null) throw new KeyNotFoundException(ErrorMessages.General.ResourceNotFound);
            _userRepository.Delete(user);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Updates user's last login info and session details.
        /// </summary>
        public async Task UpdateLoginInfoAsync(Guid userId, string? ipAddress, string refreshToken, DateTime refreshTokenExpiry, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null) throw new KeyNotFoundException(ErrorMessages.General.ResourceNotFound);

            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ipAddress;
            user.FailedLoginAttempts = 0;

            // Hash refresh token before storage
            var hashed = await _hashingService.HashAsync(refreshToken, ct);

            if (user.Session == null)
            {
                user.Session = new UserSession
                {
                    UserId = user.Id,
                    RefreshTokenHash = hashed.Hash,
                    RefreshTokenSalt = hashed.Salt,
                    RefreshTokenExpiry = refreshTokenExpiry,
                    IsOnline = true,
                    LastActiveAt = DateTime.UtcNow
                };
                // Attach session via repository Add if available
                // Using DbContext tracking since we fetched user with GetByIdAsync
            }
            else
            {
                user.Session.RefreshTokenHash = hashed.Hash;
                user.Session.RefreshTokenSalt = hashed.Salt;
                user.Session.RefreshTokenExpiry = refreshTokenExpiry;
                user.Session.IsOnline = true;
                user.Session.LastActiveAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}

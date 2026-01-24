#nullable enable
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PutZige.Application.Common;
using PutZige.Application.Common.Messages;
using PutZige.Application.DTOs.Auth;
using PutZige.Application.Interfaces;
using PutZige.Application.Settings;
using PutZige.Domain.Entities;
using PutZige.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService>? _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly IClientInfoService _clientInfoService;
        private readonly IHashingService _hashingService;

        public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, IUserService userService, IMapper mapper, IOptions<JwtSettings> jwtOptions, IClientInfoService clientInfoService, IHashingService hashingService, ILogger<AuthService>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(jwtTokenService);
            ArgumentNullException.ThrowIfNull(userService);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(jwtOptions);
            ArgumentNullException.ThrowIfNull(clientInfoService);
            ArgumentNullException.ThrowIfNull(hashingService);

            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
            _jwtSettings = jwtOptions.Value;
            _clientInfoService = clientInfoService;
            _hashingService = hashingService;
        }

        public async Task<LoginResponse> LoginAsync(string email, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("email is required", nameof(email));

            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("password is required", nameof(password));

            _logger?.LogInformation("Login attempt - Email: {Email}", email);

            var user = await _userRepository.GetByEmailWithSessionAsync(email, ct);

            if (user == null)
            {
                _logger?.LogWarning("Login failed - Non-existent email: {Email}", email);
                throw new InvalidOperationException(ErrorMessages.Authentication.InvalidCredentials);
            }

            if (!user.IsActive)
            {
                _logger?.LogWarning("Login failed - Inactive account: {Email}", email);
                throw new InvalidOperationException(ErrorMessages.Authentication.AccountInactive);
            }

            if (!user.IsEmailVerified)
            {
                _logger?.LogWarning("Login failed - Email not verified: {Email}", email);
                throw new InvalidOperationException(ErrorMessages.Authentication.EmailNotVerified);
            }

            // Auto-unlock if lockout period has expired
            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil <= DateTime.UtcNow)
            {
                user.IsLocked = false;
                user.LockedUntil = null;
                user.FailedLoginAttempts = 0;
                user.LastFailedLoginAttempt = null;
                _logger?.LogInformation("Account auto-unlocked: {Email}", email);
            }
            else if (user.IsLocked)
            {
                _logger?.LogWarning("Login failed - Account locked: {Email}", email);
                throw new InvalidOperationException(ErrorMessages.Authentication.AccountLocked);
            }

            var isValidPassword = await _hashingService.VerifyAsync(password, user.PasswordHash, user.PasswordSalt, ct);

            if (!isValidPassword)
            {
                user.FailedLoginAttempts++;
                user.LastFailedLoginAttempt = DateTime.UtcNow;

                if (user.FailedLoginAttempts >= AppConstants.Security.MaxLoginAttempts)
                {
                    user.IsLocked = true;
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(AppConstants.Security.LockoutMinutes);
                    _logger?.LogWarning("Account locked due to {Attempts} failed attempts: {Email}", user.FailedLoginAttempts, email);
                }
                else
                {
                    _logger?.LogWarning("Login failed - Invalid credentials (Attempt {Attempts}/{Max}): {Email}",
                        user.FailedLoginAttempts, AppConstants.Security.MaxLoginAttempts, email);
                }

                await _unitOfWork.SaveChangesAsync(ct);
                throw new InvalidOperationException(ErrorMessages.Authentication.InvalidCredentials);
            }

            // Successful login - reset lockout tracking
            user.FailedLoginAttempts = 0;
            user.LastFailedLoginAttempt = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = _clientInfoService.GetIpAddress();

            // Generate and hash tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, user.Username, _jwtSettings.AccessTokenExpiryMinutes, out var accessExpiresAt);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
            var hashedRefreshToken = await _hashingService.HashAsync(refreshToken, ct);

            // Update session
            UpdateUserSession(user, hashedRefreshToken.Hash, hashedRefreshToken.Salt, refreshExpiry);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger?.LogInformation("Login successful - UserId: {UserId}", user.Id);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60,
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                DisplayName = user.DisplayName
            };
        }

        private void UpdateUserSession(User user, string tokenHash, string tokenSalt, DateTime expiry)
        {
            if (user.Session == null)
            {
                user.Session = new Domain.Entities.UserSession
                {
                    UserId = user.Id,
                    RefreshTokenHash = tokenHash,
                    RefreshTokenSalt = tokenSalt,
                    RefreshTokenExpiry = expiry,
                    IsOnline = true,
                    LastActiveAt = DateTime.UtcNow
                };
            }
            else
            {
                user.Session.RefreshTokenHash = tokenHash;
                user.Session.RefreshTokenSalt = tokenSalt;
                user.Session.RefreshTokenExpiry = expiry;
                user.Session.IsOnline = true;
                user.Session.LastActiveAt = DateTime.UtcNow;
            }
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new ArgumentException("refreshToken is required", nameof(refreshToken));

            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken, ct);
            if (user == null || user.Session == null)
            {
                _logger?.LogWarning("Refresh token invalid");
                throw new InvalidOperationException(ErrorMessages.Authentication.InvalidRefreshToken);
            }

            if (!user.Session.RefreshTokenExpiry.HasValue || user.Session.RefreshTokenExpiry < DateTime.UtcNow)
            {
                _logger?.LogWarning("Refresh token expired for user {UserId}", user.Id);
                throw new InvalidOperationException(ErrorMessages.Authentication.InvalidRefreshToken);
            }

            // Verify provided refresh token with stored hash and salt
            var verified = await _hashingService.VerifyAsync(refreshToken, user.Session.RefreshTokenHash ?? string.Empty, user.Session.RefreshTokenSalt ?? string.Empty, ct);
            if (!verified)
            {
                _logger?.LogWarning("Refresh token verification failed for user {UserId}", user.Id);
                throw new InvalidOperationException(ErrorMessages.Authentication.InvalidRefreshToken);
            }

            // Generate new tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, user.Username, _jwtSettings.AccessTokenExpiryMinutes, out var accessExpiresAt);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var newRefreshExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

            var newHashed = await _hashingService.HashAsync(newRefreshToken, ct);

            user.Session.RefreshTokenHash = newHashed.Hash;
            user.Session.RefreshTokenSalt = newHashed.Salt;
            user.Session.RefreshTokenExpiry = newRefreshExpiry;
            user.Session.LastActiveAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger?.LogInformation("Refresh token rotated for user {UserId}", user.Id);

            return new RefreshTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60
            };
        }
    }
}

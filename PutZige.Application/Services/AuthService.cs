#nullable enable
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PutZige.Application.Common;
using PutZige.Application.Common.Constants;
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
        private readonly IBackgroundJobDispatcher _backgroundJobDispatcher;

        public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, IUserService userService, IMapper mapper, IOptions<JwtSettings> jwtOptions, IClientInfoService clientInfoService, IHashingService hashingService, ILogger<AuthService>? logger = null, IBackgroundJobDispatcher? backgroundJobDispatcher = null)
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
            // Ensure a background job dispatcher is always available to avoid null refs when enqueueing jobs
            _backgroundJobDispatcher = backgroundJobDispatcher ?? new NoOpBackgroundJobDispatcher();
        }

        public async Task<bool> VerifyEmailAsync(string email, string token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(ErrorMessages.Validation.EmailRequired, nameof(email));

            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException(ErrorMessages.Validation.TokenRequired, nameof(token));

            var user = await _userRepository.GetByEmailAsync(email, ct);

            if (user == null) throw new KeyNotFoundException(ErrorMessages.General.ResourceNotFound);

            if (user.IsEmailVerified) throw new InvalidOperationException(ErrorMessages.Email.AlreadyVerified);

            if (string.IsNullOrWhiteSpace(user.EmailVerificationToken) || user.EmailVerificationToken != token)
                throw new InvalidOperationException(ErrorMessages.Email.TokenInvalid);

            if (!user.EmailVerificationTokenExpiry.HasValue || user.EmailVerificationTokenExpiry.Value <= DateTime.UtcNow)
                throw new InvalidOperationException(ErrorMessages.Email.TokenExpired);

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger?.LogInformation("Email verified for user {Email}", user.Email);

            return true;
        }

        public async Task ResendVerificationEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(ErrorMessages.Validation.EmailRequired, nameof(email));

            var user = await _userRepository.GetByEmailAsync(email, ct);
            if (user == null) throw new KeyNotFoundException(ErrorMessages.General.ResourceNotFound);

            if (user.IsEmailVerified) throw new InvalidOperationException(ErrorMessages.Email.AlreadyVerified);

            var now = DateTime.UtcNow;
            if (user.LastEmailVerificationSentAt.HasValue && user.LastEmailVerificationSentAt.Value.AddHours(1) > now && user.EmailVerificationSentCount >= 3)
            {
                throw new InvalidOperationException(ErrorMessages.Email.TooManyResendAttempts);
            }

            var token = _hashingService.GenerateSecureToken(32);
            user.EmailVerificationToken = token;
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(AppConstants.Security.EmailVerificationTokenExpirationDays);
            user.EmailVerificationSentCount++;
            user.LastEmailVerificationSentAt = now;

            await _unitOfWork.SaveChangesAsync(ct);

            // Enqueue background job
            try
            {
                _backgroundJobDispatcher.EnqueueVerificationEmail(user.Email, user.Username, user.EmailVerificationToken!);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Failed to enqueue resend verification email for {Email}", user.Email);
                throw new InvalidOperationException(ErrorMessages.Email.EmailSendFailed);
            }
        }

        public async Task<LoginResponse> LoginAsync(string identifier, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException(ErrorMessages.Validation.IdentifierRequired, nameof(identifier));

            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException(ErrorMessages.Validation.PasswordRequired, nameof(password));

            _logger?.LogInformation("Login attempt - Identifier: {Identifier}", identifier);

            var user = identifier.Contains('@')
                ? await _userRepository.GetByEmailWithSessionAsync(identifier, ct)
                : await _userRepository.GetByUsernameWithSessionAsync(identifier, ct);

            if (user == null)
            {
                _logger?.LogWarning("Login failed - Non-existent identifier: {Identifier}", identifier);
                throw new InvalidOperationException(ErrorMessages.Authentication.InvalidCredentials);
            }

            if (!user.IsActive)
            {
                _logger?.LogWarning("Login failed - Inactive account: {Identifier}", identifier);
                throw new InvalidOperationException(ErrorMessages.Authentication.AccountInactive);
            }

            if (!user.IsEmailVerified)
            {
                _logger?.LogWarning("Login failed - Email not verified: {Identifier}", identifier);
                throw new InvalidOperationException(ErrorMessages.Authentication.EmailNotVerified);
            }

            // Auto-unlock if lockout period has expired
            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil <= DateTime.UtcNow)
            {
                user.IsLocked = false;
                user.LockedUntil = null;
                user.FailedLoginAttempts = 0;
                user.LastFailedLoginAttempt = null;
                _logger?.LogInformation("Account auto-unlocked: {Identifier}", identifier);
            }
            else if (user.IsLocked)
            {
                _logger?.LogWarning("Login failed - Account locked: {Identifier}", identifier);
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
                    _logger?.LogWarning("Account locked due to {Attempts} failed attempts: {Identifier}", user.FailedLoginAttempts, identifier);
                }
                else
                {
                    _logger?.LogWarning("Login failed - Invalid credentials (Attempt {Attempts}/{Max}): {Identifier}",
                        user.FailedLoginAttempts, AppConstants.Security.MaxLoginAttempts, identifier);
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
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new ArgumentException(ErrorMessages.Validation.RefreshTokenRequired, nameof(refreshToken));

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

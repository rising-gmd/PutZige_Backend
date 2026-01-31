#nullable enable
using System;
using System.Globalization;
using System.Net.Mail;
using System.Security.Claims;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PutZige.Application.Common.Messages;
using PutZige.Application.Interfaces;
using PutZige.Infrastructure.Validation;

namespace PutZige.Infrastructure.Services;

/// <summary>
/// Resolves information about the current authenticated user from the HTTP context.
/// This implementation validates JWT claims (format and expiration) and throws <see cref="UnauthorizedAccessException"/>
/// on validation failures.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService>? _logger;

    // Validator is stateless and safe for concurrent use.
    private static readonly AuthClaimsValidator _claimsValidator = new();

    /// <summary>
    /// Initializes a new instance of <see cref="CurrentUserService"/>.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContextAccessor"/> is null.</exception>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService>? logger = null)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's ID from JWT claims. Validates claim presence, format, and token expiration.
    /// Throws <see cref="UnauthorizedAccessException"/> on any validation failure.
    /// </summary>
    /// <returns>The user's <see cref="Guid"/> identifier.</returns>
    /// <exception cref="UnauthorizedAccessException">When the user is not authenticated or claims are invalid/expired.</exception>
    public Guid GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null || principal.Identity is null || !principal.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException(ErrorMessages.General.UnauthorizedAccess);

        var dto = new AuthClaimsDto
        {
            UserId = principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Email = principal.FindFirst("email")?.Value,
            Username = principal.FindFirst("username")?.Value,
            Exp = principal.FindFirst("exp")?.Value
        };

        ValidateClaimsOrThrow(dto);

        // validator guarantees valid Guid
        return Guid.Parse(dto.UserId!);
    }

    /// <summary>
    /// Attempts to get the current user's ID from JWT claims without throwing on validation failures.
    /// Returns null when unauthenticated, missing, malformed, or expired.
    /// </summary>
    public Guid? TryGetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null || principal.Identity is null || !principal.Identity.IsAuthenticated)
            return null;

        var userId = principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        if (!Guid.TryParse(userId, out var id))
        {
            _logger?.LogWarning("Invalid user ID claim format: {Claim}", userId);
            return null;
        }

        var expClaim = principal.FindFirst("exp")?.Value;
        if (!string.IsNullOrWhiteSpace(expClaim) && TryParseUnixEpochSeconds(expClaim, out var exp) && DateTimeOffset.UtcNow >= exp)
        {
            _logger?.LogWarning("Token expired for user {UserId}", id);
            return null;
        }

        return id;
    }

    /// <summary>
    /// Gets the current user's email claim if present and valid.
    /// Returns null when no email claim exists. Throws <see cref="UnauthorizedAccessException"/> when the email claim is present but invalid.
    /// </summary>
    /// <returns>Email string or null.</returns>
    public string? GetUserEmail()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null || principal.Identity is null || !principal.Identity.IsAuthenticated)
            return null;

        var email = principal.FindFirst("email")?.Value;
        if (string.IsNullOrWhiteSpace(email))
            return null;

        try
        {
            _ = new MailAddress(email);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Invalid email claim format: {Email}", email);
            throw new UnauthorizedAccessException(ErrorMessages.General.UnauthorizedAccess);
        }

        return email;
    }

    /// <summary>
    /// Gets the current user's username claim or falls back to identity name.
    /// </summary>
    /// <returns>Username string or null.</returns>
    public string? GetUserName()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null)
            return null;

        var username = principal.FindFirst("username")?.Value;
        if (!string.IsNullOrWhiteSpace(username))
            return username;

        return principal.Identity?.Name;
    }

    /// <summary>
    /// Checks if the current user is authenticated and token (if present) is not expired.
    /// Returns false for invalid or expired tokens.
    /// </summary>
    public bool IsAuthenticated()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null)
            return false;

        var isAuth = principal.Identity?.IsAuthenticated ?? false;
        if (!isAuth)
            return false;

        var exp = principal.FindFirst("exp")?.Value;
        if (string.IsNullOrWhiteSpace(exp))
            return true;

        if (!TryParseUnixEpochSeconds(exp, out var expDate))
        {
            _logger?.LogWarning("Invalid exp claim format: {Exp}", exp);
            return false;
        }

        var notExpired = DateTimeOffset.UtcNow < expDate;
        if (!notExpired)
            _logger?.LogInformation("Token expired at {ExpDate}", expDate);

        return notExpired;
    }

    /// <summary>
    /// Validates claim DTO using FluentValidation and throws <see cref="UnauthorizedAccessException"/> when validation fails.
    /// </summary>
    /// <param name="dto">Claim values to validate.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when validation fails.</exception>
    private static void ValidateClaimsOrThrow(AuthClaimsDto dto)
    {
        ValidationResult result = _claimsValidator.Validate(dto);
        if (!result.IsValid)
        {
            // Intentionally avoid leaking validation details to callers for security
            throw new UnauthorizedAccessException(ErrorMessages.General.UnauthorizedAccess);
        }
    }

    /// <summary>
    /// Attempts to parse a Unix epoch seconds string to <see cref="DateTimeOffset"/>.
    /// Accepts integer or floating number strings; returns false on parse failure.
    /// </summary>
    /// <param name="epochSeconds">Epoch seconds string.</param>
    /// <param name="dateTime">Parsed DateTimeOffset value when successful.</param>
    /// <returns>True when parse successful; otherwise false.</returns>
    private static bool TryParseUnixEpochSeconds(string epochSeconds, out DateTimeOffset dateTime)
    {
        dateTime = default;
        if (long.TryParse(epochSeconds, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
        {
            try
            {
                dateTime = DateTimeOffset.FromUnixTimeSeconds(seconds);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        if (double.TryParse(epochSeconds, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
        {
            var secs = Convert.ToInt64(Math.Floor(d));
            try
            {
                dateTime = DateTimeOffset.FromUnixTimeSeconds(secs);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        return false;
    }
}

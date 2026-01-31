#nullable enable
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PutZige.Application.Interfaces;
using PutZige.Application.Common.Messages;

namespace PutZige.Infrastructure.Services;

/// <summary>
/// Resolves information about the current authenticated user from the HTTP context.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService>? _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService>? logger = null)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger;
    }

    public Guid GetUserId()
    {
        if (!IsAuthenticated())
            throw new UnauthorizedAccessException(ErrorMessages.General.UnauthorizedAccess);

        // Try "sub" first, then ClaimTypes.NameIdentifier as fallback
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                 ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(claim))
        {
            _logger?.LogError("Missing sub claim for authenticated user");
            throw new UnauthorizedAccessException(ErrorMessages.General.UnauthorizedAccess);
        }

        if (!Guid.TryParse(claim, out var id))
        {
            _logger?.LogError("Invalid user ID claim format: {Claim}", claim);
            throw new UnauthorizedAccessException(ErrorMessages.General.UnauthorizedAccess);
        }

        return id;
    }

    public Guid? TryGetUserId()
    {
        if (!IsAuthenticated())
            return null;

        // Same fallback logic
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                 ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(claim))
            return null;

        if (!Guid.TryParse(claim, out var id))
        {
            _logger?.LogWarning("Invalid user ID claim format: {Claim}", claim);
            return null;
        }

        return id;
    }

    /// <inheritdoc />
    public string? GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
    }

    /// <inheritdoc />
    public string? GetUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    /// <inheritdoc />
    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}

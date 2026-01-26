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

    /// <inheritdoc />
    public Guid GetUserId()
    {
        if (!IsAuthenticated())
            throw new InvalidOperationException(ErrorMessages.General.UnauthorizedAccess);

        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(claim))
        {
            _logger?.LogError("Missing sub claim for authenticated user");
            throw new InvalidOperationException(ErrorMessages.General.UnauthorizedAccess);
        }

        if (!Guid.TryParse(claim, out var id))
        {
            _logger?.LogError("Invalid user ID claim format: {Claim}", claim);
            throw new InvalidOperationException(ErrorMessages.General.UnauthorizedAccess);
        }

        return id;
    }

    /// <inheritdoc />
    public Guid? TryGetUserId()
    {
        if (!IsAuthenticated())
            return null;

        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
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

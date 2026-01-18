#nullable enable
using System;

namespace PutZige.Application.DTOs.Auth
{
    /// <summary>
    /// Placeholder for future login request model.
    /// </summary>
    public sealed class LoginRequest
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}

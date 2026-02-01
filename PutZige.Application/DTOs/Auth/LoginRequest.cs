#nullable enable
using System;

namespace PutZige.Application.DTOs.Auth
{
    /// <summary>
    /// Request payload for logging in a user.
    /// </summary>
    public sealed class LoginRequest
    {
        public string Identifier { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}

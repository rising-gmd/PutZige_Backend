#nullable enable
using System;

namespace PutZige.Application.DTOs.Auth
{
    /// <summary>
    /// Request payload for logging in a user.
    /// </summary>
    public sealed class LoginRequest
    {
        public string Email { get; init; }
        public string Password { get; init; }
    }
}

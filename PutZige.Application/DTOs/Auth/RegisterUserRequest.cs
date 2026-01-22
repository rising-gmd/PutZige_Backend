#nullable enable
using System;

namespace PutZige.Application.DTOs.Auth
{
    /// <summary>
    /// Request payload for registering a new user.
    /// </summary>
    public sealed class RegisterUserRequest
    {
        public string? Email { get; init; }
        public string? Username { get; init; }
        public string? DisplayName { get; init; }
        public string? Password { get; init; }
        public string? ConfirmPassword { get; init; }
    }
}

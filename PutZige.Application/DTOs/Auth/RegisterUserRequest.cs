#nullable enable
using System;

namespace PutZige.Application.DTOs.Auth
{
    /// <summary>
    /// Request payload for registering a new user.
    /// </summary>
    public sealed class RegisterUserRequest
    {
        public required string Email { get; init; }
        public required string Username { get; init; }
        public required string DisplayName { get; init; }
        public required string Password { get; init; }
        public required string ConfirmPassword { get; init; }
    }
}

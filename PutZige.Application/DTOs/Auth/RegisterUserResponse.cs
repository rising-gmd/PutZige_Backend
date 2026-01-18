#nullable enable
using System;

namespace PutZige.Application.DTOs.Auth
{
    /// <summary>
    /// Response payload returned after successful registration.
    /// </summary>
    public sealed class RegisterUserResponse
    {
        public required Guid UserId { get; init; }
        public required string Email { get; init; }
        public required string Username { get; init; }
        public required string DisplayName { get; init; }
        public required bool IsEmailVerified { get; init; }
        public required DateTime CreatedAt { get; init; }
    }
}

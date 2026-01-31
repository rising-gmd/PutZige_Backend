#nullable enable
using System;

namespace PutZige.Application.DTOs.Auth
{
    public sealed class LoginResponse
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public required int ExpiresIn { get; init; }
        public required Guid UserId { get; init; }
        public required string Email { get; init; }
        public required string Username { get; init; }
        public string? DisplayName { get; init; }
    }
}

#nullable enable
using System;

namespace PutZige.Application.DTOs.Auth
{
    public sealed class RefreshTokenResponse
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public required int ExpiresIn { get; init; }
    }
}

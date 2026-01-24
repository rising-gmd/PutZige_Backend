#nullable enable
using System;

namespace PutZige.Application.Settings
{
    public sealed class JwtSettings
    {
        public const string SectionName = "JwtSettings";
        public required string Secret { get; init; }
        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public int AccessTokenExpiryMinutes { get; init; } = 15;
        public int RefreshTokenExpiryDays { get; init; } = 7;
    }
}

using System;

namespace PutZige.Domain.Entities
{
    public class UserSession : BaseEntity
    {
        public Guid UserId { get; set; }

        public bool IsOnline { get; set; } = false;
        public DateTime? LastActiveAt { get; set; }

        // Device tokens stored as JSON array
        public string? DeviceTokens { get; set; }

        // JWT refresh token details
        public string? RefreshTokenHash { get; set; }
        public string? RefreshTokenSalt { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public User? User { get; set; }
    }
}

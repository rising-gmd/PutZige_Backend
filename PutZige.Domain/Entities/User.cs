using System;

namespace PutZige.Domain.Entities
{
    /// <summary>
    /// Represents an application user.
    /// </summary>
    public class User : BaseEntity
    {
        // Authentication
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }

        // Profile
        public string Username { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // Password Reset
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public int PasswordResetAttempts { get; set; } = 0;
        public DateTime? LastPasswordResetAttempt { get; set; }

        // Two-Factor Authentication
        public bool IsTwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; }
        public string? TwoFactorRecoveryCodes { get; set; }

        // Account Status
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockedUntil { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLoginAttempt { get; set; }

        // Session & Security
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public DateTime? LastActiveAt { get; set; }

        // Privacy
        public bool IsOnline { get; set; } = false;
        public bool ShowOnlineStatus { get; set; } = true;
        public bool AllowFriendRequests { get; set; } = true;

        // Rate Limiting
        public int MessagesSentToday { get; set; } = 0;
        public DateTime? LastMessageSentAt { get; set; }
        public int ApiCallsToday { get; set; } = 0;
        public DateTime? RateLimitResetAt { get; set; }

        // Metadata
        public string? DeviceTokens { get; set; }
        public string? Preferences { get; set; }
        public string? Metadata { get; set; }
    }
}

// File: src/PutZige.Domain/Entities/User.cs
using System;
using System.Collections.Generic;

namespace PutZige.Domain.Entities
{
    /// <summary>
    /// Represents an application user.
    /// </summary>
    public class User
    {
        // Primary Key
        public Guid Id { get; set; }

        // Authentication
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }

        // Profile
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; } = string.Empty;

        // Password Reset
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public int PasswordResetAttempts { get; set; } = 0;
        public DateTime? LastPasswordResetAttempt { get; set; }

        // Two-Factor Authentication
        public bool IsTwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; } = string.Empty;
        public string? TwoFactorRecoveryCodes { get; set; } = string.Empty;

        // Account Status
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockedUntil { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLoginAttempt { get; set; }

        // Session & Security
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; } = string.Empty;
        public DateTime? LastActiveAt { get; set; }
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiry { get; set; }

        // Privacy
        public bool IsOnline { get; set; } = false;
        public bool ShowOnlineStatus { get; set; } = true;
        public bool AllowFriendRequests { get; set; } = true;

        // Rate Limiting
        public int MessagesSentToday { get; set; } = 0;
        public DateTime? LastMessageSentAt { get; set; }
        public int ApiCallsToday { get; set; } = 0;
        public DateTime? RateLimitResetAt { get; set; }

        // Audit & Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Metadata
        public string? DeviceTokens { get; set; } = string.Empty; // JSON array
        public string? Preferences { get; set; } = string.Empty; // JSON object
        public string? Metadata { get; set; } = string.Empty; // JSON object
    }
}

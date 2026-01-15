using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EntityTypeConfiguration for User entity.
    /// </summary>
    public class UserConfiguration : BaseEntityConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);
            builder.ToTable("Users");

            // Authentication
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.IsEmailVerified).HasDefaultValue(false);
            builder.Property(u => u.EmailVerificationToken).HasMaxLength(256);

            // Profile
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.HasIndex(u => u.Username).IsUnique();
            builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Bio).HasMaxLength(500);
            builder.Property(u => u.ProfilePictureUrl).HasMaxLength(512);

            // Password Reset
            builder.Property(u => u.PasswordResetToken).HasMaxLength(256);
            builder.Property(u => u.PasswordResetAttempts).HasDefaultValue(0);

            // Two-Factor Authentication
            builder.Property(u => u.IsTwoFactorEnabled).HasDefaultValue(false);
            builder.Property(u => u.TwoFactorSecret).HasMaxLength(256);
            builder.Property(u => u.TwoFactorRecoveryCodes).HasMaxLength(1024);

            // Account Status
            builder.Property(u => u.IsActive).HasDefaultValue(true);
            builder.Property(u => u.IsLocked).HasDefaultValue(false);
            builder.Property(u => u.FailedLoginAttempts).HasDefaultValue(0);

            // Privacy
            builder.Property(u => u.IsOnline).HasDefaultValue(false);
            builder.Property(u => u.ShowOnlineStatus).HasDefaultValue(true);
            builder.Property(u => u.AllowFriendRequests).HasDefaultValue(true);

            // Rate Limiting
            builder.Property(u => u.MessagesSentToday).HasDefaultValue(0);
            builder.Property(u => u.ApiCallsToday).HasDefaultValue(0);

            // Metadata
            builder.Property(u => u.DeviceTokens).HasMaxLength(1024);
            builder.Property(u => u.Preferences).HasMaxLength(2048);
            builder.Property(u => u.Metadata).HasMaxLength(2048);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;
using PutZige.Application.Common.Constants;

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
            builder.Property(u => u.Email).IsRequired().HasMaxLength(AppConstants.Validation.MaxEmailLength); // 255
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            builder.Property(u => u.IsEmailVerified).HasDefaultValue(false);
            builder.Property(u => u.EmailVerificationToken).HasMaxLength(256);

            // Profile
            builder.Property(u => u.Username).IsRequired().HasMaxLength(AppConstants.Validation.MaxUsernameLength); // 50
            builder.HasIndex(u => u.Username).IsUnique();
            builder.Property(u => u.DisplayName).HasMaxLength(AppConstants.Validation.MaxDisplayNameLength); // 100
            builder.Property(u => u.Bio).HasMaxLength(AppConstants.Validation.MaxShortTextLength); // 500
            builder.Property(u => u.ProfilePictureUrl).HasMaxLength(AppConstants.Validation.MaxUrlLength); // 2048

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

            // Session & Security
            builder.Property(u => u.LastLoginAt);
            builder.Property(u => u.LastLoginIp).HasMaxLength(50);

            // Navigation configuration for normalized related entities
            builder.HasOne(u => u.Settings).WithOne(s => s.User).HasForeignKey<UserSettings>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(u => u.Session).WithOne(s => s.User).HasForeignKey<UserSession>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(u => u.RateLimit).WithOne(r => r.User).HasForeignKey<UserRateLimit>(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(u => u.Metadata).WithOne(m => m.User).HasForeignKey<UserMetadata>(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
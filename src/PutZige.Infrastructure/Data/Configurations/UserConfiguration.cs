// File: src/PutZige.Infrastructure/Data/Configurations/UserConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity configuration for User.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Primary Key
            builder.HasKey(u => u.Id);

            // Required Fields & Max Lengths
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(u => u.DisplayName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(u => u.Bio)
                .HasMaxLength(500);
            builder.Property(u => u.ProfilePictureUrl)
                .HasMaxLength(500);
            builder.Property(u => u.TwoFactorSecret)
                .HasMaxLength(256);
            builder.Property(u => u.RefreshToken)
                .HasMaxLength(512);

            // Indexes
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.IsDeleted);

            // Default Values
            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnType("datetime2");
            builder.Property(u => u.IsDeleted)
                .HasDefaultValue(false);
            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);
            builder.Property(u => u.IsTwoFactorEnabled)
                .HasDefaultValue(false);
            builder.Property(u => u.IsOnline)
                .HasDefaultValue(false);

            // Column Types
            builder.Property(u => u.Email).HasColumnType("nvarchar(256)");
            builder.Property(u => u.PasswordHash).HasColumnType("nvarchar(256)");
            builder.Property(u => u.Username).HasColumnType("nvarchar(50)");
            builder.Property(u => u.DisplayName).HasColumnType("nvarchar(100)");
            builder.Property(u => u.Bio).HasColumnType("nvarchar(500)");
            builder.Property(u => u.ProfilePictureUrl).HasColumnType("nvarchar(500)");
            builder.Property(u => u.TwoFactorSecret).HasColumnType("nvarchar(256)");
            builder.Property(u => u.RefreshToken).HasColumnType("nvarchar(512)");
            builder.Property(u => u.DeviceTokens).HasColumnType("nvarchar(max)");
            builder.Property(u => u.Preferences).HasColumnType("nvarchar(max)");
            builder.Property(u => u.Metadata).HasColumnType("nvarchar(max)");
        }
    }
}

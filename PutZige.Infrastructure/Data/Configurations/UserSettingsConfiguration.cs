using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;
using PutZige.Application.Common.Constants;

namespace PutZige.Infrastructure.Data.Configurations
{
    public class UserSettingsConfiguration : BaseEntityConfiguration<UserSettings>
    {
        public override void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            base.Configure(builder);
            builder.ToTable("UserSettings");
            builder.Property(s => s.ShowOnlineStatus).HasDefaultValue(true);
            builder.Property(s => s.AllowFriendRequests).HasDefaultValue(true);
            builder.Property(s => s.EmailNotifications).HasDefaultValue(true);
            builder.Property(s => s.PushNotifications).HasDefaultValue(true);
            builder.Property(s => s.Theme).HasMaxLength(AppConstants.Validation.MaxThemeLength);
            builder.Property(s => s.Language).HasMaxLength(AppConstants.Validation.MaxLanguageLength);
            builder.Property(s => s.Preferences).HasMaxLength(AppConstants.Validation.MaxLongTextLength);
            builder.HasIndex(s => s.UserId).IsUnique();
            builder.HasOne(s => s.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<UserSettings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
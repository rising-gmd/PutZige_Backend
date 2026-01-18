using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Data.Configurations
{
    public class UserSettingsConfiguration : BaseEntityConfiguration<UserSettings>
    {
        public override void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            base.Configure(builder);
            builder.ToTable("UserSettings");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.ShowOnlineStatus).HasDefaultValue(true);
            builder.Property(s => s.AllowFriendRequests).HasDefaultValue(true);
            builder.Property(s => s.EmailNotifications).HasDefaultValue(true);
            builder.Property(s => s.PushNotifications).HasDefaultValue(true);

            builder.Property(s => s.Theme).HasMaxLength(50);
            builder.Property(s => s.Language).HasMaxLength(10);
            builder.Property(s => s.Preferences).HasMaxLength(2048);

            builder.HasIndex(s => s.UserId).IsUnique();

            builder.HasOne(s => s.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<UserSettings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

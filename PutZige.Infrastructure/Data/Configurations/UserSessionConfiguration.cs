using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Data.Configurations
{
    public class UserSessionConfiguration : BaseEntityConfiguration<UserSession>
    {
        public override void Configure(EntityTypeBuilder<UserSession> builder)
        {
            base.Configure(builder);
            builder.ToTable("UserSessions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.IsOnline).HasDefaultValue(false);
            builder.Property(s => s.DeviceTokens).HasMaxLength(2048);
            builder.Property(s => s.CurrentRefreshToken).HasMaxLength(512);

            builder.HasIndex(s => s.UserId).IsUnique();
            builder.HasIndex(s => s.LastActiveAt);

            builder.HasOne(s => s.User)
                .WithOne(u => u.Session)
                .HasForeignKey<UserSession>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

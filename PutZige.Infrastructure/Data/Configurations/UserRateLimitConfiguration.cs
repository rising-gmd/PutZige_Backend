using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Data.Configurations
{
    public class UserRateLimitConfiguration : BaseEntityConfiguration<UserRateLimit>
    {
        public override void Configure(EntityTypeBuilder<UserRateLimit> builder)
        {
            base.Configure(builder);
            builder.ToTable("UserRateLimits");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.MessagesSentToday).HasDefaultValue(0);
            builder.Property(r => r.ApiCallsToday).HasDefaultValue(0);

            builder.HasIndex(r => r.UserId).IsUnique();
            builder.HasIndex(r => r.RateLimitResetAt);

            builder.HasOne(r => r.User)
                .WithOne(u => u.RateLimit)
                .HasForeignKey<UserRateLimit>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

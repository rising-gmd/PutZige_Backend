using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Data.Configurations
{
    public class UserMetadataConfiguration : BaseEntityConfiguration<UserMetadata>
    {
        public override void Configure(EntityTypeBuilder<UserMetadata> builder)
        {
            base.Configure(builder);
            builder.ToTable("UserMetadata");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Metadata).HasMaxLength(4000);

            builder.HasIndex(m => m.UserId).IsUnique();

            builder.HasOne(m => m.User)
                .WithOne(u => u.Metadata)
                .HasForeignKey<UserMetadata>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

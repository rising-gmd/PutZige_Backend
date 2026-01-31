using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Data.Configurations
{
    public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.IsDeleted);
            builder.HasIndex(e => e.CreatedAt);
            builder.Property(e => e.Id).IsRequired();
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.IsDeleted).IsRequired();
            builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        }
    }
}
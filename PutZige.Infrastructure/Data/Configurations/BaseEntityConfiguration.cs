using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Base configuration for all entities inheriting from BaseEntity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.IsDeleted);
            builder.HasIndex(e => e.CreatedAt);
            builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(e => e.IsDeleted).HasDefaultValue(false);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.IsDeleted).IsRequired();
        }
    }
}

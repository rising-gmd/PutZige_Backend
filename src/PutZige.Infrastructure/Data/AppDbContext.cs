// File: src/PutZige.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using PutZige.Domain.Entities;
using PutZige.Infrastructure.Data.Configurations;

namespace PutZige.Infrastructure.Data
{
    /// <summary>
    /// Application database context.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        }
    }
}

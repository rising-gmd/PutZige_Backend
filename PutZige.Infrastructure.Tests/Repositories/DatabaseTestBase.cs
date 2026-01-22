// PutZige.Infrastructure.Tests/Repositories/DatabaseTestBase.cs
#nullable enable
using System;
using Microsoft.EntityFrameworkCore;
using PutZige.Infrastructure.Data;
using PutZige.Infrastructure.Repositories;
using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Repositories;
using PutZige.Domain.Entities;

namespace PutZige.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Base class that provides an in-memory AppDbContext and UnitOfWork for repository tests.
    /// </summary>
    public abstract class DatabaseTestBase : IDisposable
    {
        protected readonly string DatabaseName;
        protected readonly AppDbContext Context;
        protected readonly IUnitOfWork UnitOfWork;

        protected DatabaseTestBase()
        {
            DatabaseName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(DatabaseName)
                .Options;

            Context = new AppDbContext(options);
            UnitOfWork = new UnitOfWork(Context);

            // Ensure database is created
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            // Ensure database is removed and context disposed
            try
            {
                Context.Database.EnsureDeleted();
            }
            catch { }
            Context.Dispose();
        }
    }
}

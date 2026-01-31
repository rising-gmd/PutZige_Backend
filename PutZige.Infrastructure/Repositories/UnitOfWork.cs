// File: src/PutZige.Infrastructure/Repositories/UnitOfWork.cs

using PutZige.Domain.Interfaces;
using PutZige.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);
    }
}

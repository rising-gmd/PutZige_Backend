// File: src/PutZige.Domain/Interfaces/IUnitOfWork.cs

using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

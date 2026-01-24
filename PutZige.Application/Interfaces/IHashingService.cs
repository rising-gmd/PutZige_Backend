using System.Threading;
using System.Threading.Tasks;
using PutZige.Application.DTOs;

namespace PutZige.Application.Interfaces
{
    public interface IHashingService
    {
        Task<HashedValue> HashAsync(string plainText, CancellationToken ct = default);
        Task<bool> VerifyAsync(string plainText, string hash, string salt, CancellationToken ct = default);
        string GenerateSecureToken(int byteLength = 32);
    }
}
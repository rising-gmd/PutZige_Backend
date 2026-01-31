#nullable enable
using System.Threading;
using System.Threading.Tasks;
using PutZige.Application.DTOs.Auth;

namespace PutZige.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password, CancellationToken ct = default);
        Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    }
}

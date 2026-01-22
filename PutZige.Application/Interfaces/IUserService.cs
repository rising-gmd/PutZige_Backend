#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using PutZige.Application.DTOs.Auth;

namespace PutZige.Application.Interfaces
{
    /// <summary>
    /// Service contract for user registration and management.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Registers a new user with validation and hashing and returns a response DTO.
        /// </summary>
        Task<RegisterUserResponse> RegisterUserAsync(
            string email,
            string username,
            string displayName,
            string password,
            CancellationToken ct = default);

        /// <summary>
        /// Gets a user by email.
        /// </summary>
        Task<RegisterUserResponse?> GetUserByEmailAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Soft deletes a user by id.
        /// </summary>
        Task SoftDeleteUserAsync(Guid userId, CancellationToken ct = default);

    }
}

#nullable enable
using System;

namespace PutZige.Application.Interfaces
{
    /// <summary>
    /// Service for accessing current authenticated user information from HTTP context.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the current user's ID from JWT claims.
        /// </summary>
        Guid GetUserId();
        
        /// <summary>
        /// Attempts to get the current user's ID from JWT claims.
        /// </summary>
        Guid? TryGetUserId();
        
        /// <summary>
        /// Gets the current user's email from JWT claims.
        /// </summary>
        string? GetUserEmail();
        
        /// <summary>
        /// Gets the current user's username from JWT claims.
        /// </summary>
        string? GetUserName();
        
        /// <summary>
        /// Checks if the current user is authenticated.
        /// </summary>
        bool IsAuthenticated();
    }
}

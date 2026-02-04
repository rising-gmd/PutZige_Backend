using System;

namespace PutZige.Application.Interfaces
{
    /// <summary>
    /// Provides testable abstraction for system time operations.
    /// Enables time-travel in tests for expiry/lockout/rate-limit scenarios.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Gets current UTC time. Use this instead of DateTime.UtcNow everywhere.
        /// </summary>
        DateTime UtcNow { get; }
    }
}

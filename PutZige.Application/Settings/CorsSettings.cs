#nullable enable
using System.Collections.Generic;

namespace PutZige.Application.Settings
{
    /// <summary>
    /// Strongly-typed CORS settings bound from configuration.
    /// </summary>
    public sealed class CorsSettings
    {
        /// <summary>
        /// Section name in configuration files.
        /// </summary>
        public const string SectionName = "CorsSettings";

        /// <summary>
        /// Named policy to register and use. Default is "Default".
        /// </summary>
        public string PolicyName { get; init; } = "Default";

        /// <summary>
        /// Allowed origins. Use exact origins (scheme + host + optional port). In Development, wildcard "*" is allowed.
        /// </summary>
        public List<string> AllowedOrigins { get; init; } = new();

        /// <summary>
        /// Allowed headers. Defaults to allow common headers and "*".
        /// </summary>
        public List<string> AllowedHeaders { get; init; } = new() { "*" };

        /// <summary>
        /// Allowed methods. Defaults to GET, POST, PUT, DELETE, OPTIONS, PATCH.
        /// </summary>
        public List<string> AllowedMethods { get; init; } = new() { "GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH" };

        /// <summary>
        /// Whether credentials are allowed (cookies, Authorization header). Keep false in production unless required.
        /// </summary>
        public bool AllowCredentials { get; init; } = false;

        /// <summary>
        /// Preflight max age in seconds to cache preflight responses. Default 600 (10 minutes).
        /// </summary>
        public int PreflightMaxAgeSeconds { get; init; } = 600;
    }
}

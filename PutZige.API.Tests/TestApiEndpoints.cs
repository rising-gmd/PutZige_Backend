// PutZige.API.Tests/TestApiEndpoints.cs
#nullable enable
namespace PutZige.API.Tests
{
    internal static class TestApiEndpoints
    {
        // Auth endpoints
        public const string AuthLogin = "/api/v1/auth/login";
        public const string AuthRefreshToken = "/api/v1/auth/refresh-token";

        // Users endpoints
        public const string Users = "/api/v1/users";

        // Health endpoint
        public const string Health = "/api/v1/health";
    }
}

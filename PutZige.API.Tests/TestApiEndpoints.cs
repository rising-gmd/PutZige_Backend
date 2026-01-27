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

        // SignalR hubs
        public const string ChatHub = "/hubs/chat";

        // Messages endpoints
        public const string Messages = "/api/v1/messages";
        public const string MessagesConversation = "/api/v1/messages/conversation";
        public const string MessageRead = "/api/v1/messages/{0}/read";
        public const string MessageMarkAsRead = "/api/v1/messages/{0}/mark-as-read";
        public const string MessageById = "/api/v1/messages/{0}";
    }
}

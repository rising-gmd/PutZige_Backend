using System;

namespace PutZige.Domain.Entities
{
    public class UserSettings : BaseEntity
    {
        public Guid UserId { get; set; }

        public bool ShowOnlineStatus { get; set; } = true;
        public bool AllowFriendRequests { get; set; } = true;

        // New notification preferences
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;

        // UI preferences
        public string? Theme { get; set; }
        public string? Language { get; set; }

        // Extensible preferences blob
        public string? Preferences { get; set; }

        public User? User { get; set; }
    }
}

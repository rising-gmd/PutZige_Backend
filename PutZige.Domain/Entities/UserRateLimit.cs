using System;

namespace PutZige.Domain.Entities
{
    public class UserRateLimit : BaseEntity
    {
        public Guid UserId { get; set; }

        public int MessagesSentToday { get; set; } = 0;
        public DateTime? LastMessageSentAt { get; set; }

        public int ApiCallsToday { get; set; } = 0;
        public DateTime? RateLimitResetAt { get; set; }

        public User? User { get; set; }
    }
}

using System;

namespace PutZige.Domain.Entities
{
    public class UserMetadata : BaseEntity
    {
        public Guid UserId { get; set; }

        public string? Metadata { get; set; }

        public User? User { get; set; }
    }
}

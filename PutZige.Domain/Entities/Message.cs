using System;

namespace PutZige.Domain.Entities
{
    public class Message : BaseEntity
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }

        public string MessageText { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public User? Sender { get; set; }
        public User? Receiver { get; set; }
    }
}

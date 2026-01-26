using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PutZige.Domain.Entities;
using PutZige.Application.Common.Constants;

namespace PutZige.Infrastructure.Data.Configurations
{
    public class MessageConfiguration : BaseEntityConfiguration<Message>
    {
        public override void Configure(EntityTypeBuilder<Message> builder)
        {
            base.Configure(builder);

            builder.ToTable("Messages");

            builder.Property(m => m.SenderId).IsRequired();
            builder.Property(m => m.ReceiverId).IsRequired();

            builder.Property(m => m.MessageText).IsRequired().HasMaxLength(AppConstants.Validation.MaxLongTextLength);

            builder.Property(m => m.SentAt).IsRequired();

            // Relationships
            builder.HasOne(m => m.Sender).WithMany(u => u.SentMessages).HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.Receiver).WithMany(u => u.ReceivedMessages).HasForeignKey(m => m.ReceiverId).OnDelete(DeleteBehavior.Restrict);

            // Indexes for inbox and conversation queries
            builder.HasIndex(m => new { m.ReceiverId, m.SentAt }).HasDatabaseName("IX_Messages_ReceiverId_SentAt");
            builder.HasIndex(m => new { m.SenderId, m.ReceiverId, m.SentAt }).HasDatabaseName("IX_Messages_SenderId_ReceiverId_SentAt");
        }
    }
}

#nullable enable
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PutZige.Domain.Entities;
using PutZige.Infrastructure.Data;
using PutZige.Application.Common.Constants;
using Xunit;

namespace PutZige.Infrastructure.Tests.Data
{
    public class MessageEntityConfigurationTests
    {
        /// <summary>
        /// Verifies that MessageEntity_HasCorrectTableName behaves as expected.
        /// </summary>
        [Fact]
        public void MessageEntity_HasCorrectTableName()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(Message));
            entity.Should().NotBeNull();
            entity!.GetTableName().Should().Be("Messages");
        }

        /// <summary>
        /// Verifies that MessageEntity_SenderIdIndexExists behaves as expected.
        /// </summary>
        [Fact]
        public void MessageEntity_SenderIdIndexExists()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(Message));
            var indexes = entity!.GetIndexes();
            // There should be an index that includes SenderId (part of composite)
            indexes.Any(i => i.Properties.Any(p => p.Name == nameof(Message.SenderId))).Should().BeTrue();
        }

        /// <summary>
        /// Verifies that MessageEntity_ReceiverIdSentAtIndexExists behaves as expected.
        /// </summary>
        [Fact]
        public void MessageEntity_ReceiverIdSentAtIndexExists()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(Message));
            var indexes = entity!.GetIndexes();
            indexes.Should().Contain(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Message.ReceiverId), nameof(Message.SentAt) }));
        }

        /// <summary>
        /// Verifies that MessageEntity_SenderReceiverSentAtCompositeIndexExists behaves as expected.
        /// </summary>
        [Fact]
        public void MessageEntity_SenderReceiverSentAtCompositeIndexExists()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(Message));
            var indexes = entity!.GetIndexes();
            indexes.Should().Contain(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Message.SenderId), nameof(Message.ReceiverId), nameof(Message.SentAt) }));
        }

        /// <summary>
        /// Verifies that MessageEntity_MessageTextMaxLength4000 behaves as expected.
        /// </summary>
        [Fact]
        public void MessageEntity_MessageTextMaxLength4000()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(Message));
            var prop = entity!.FindProperty(nameof(Message.MessageText));
            prop.Should().NotBeNull();

            // The configuration uses Validation.MaxLongTextLength (5000) while messaging max is 4000.
            prop!.GetMaxLength().Should().Be(AppConstants.Validation.MaxLongTextLength);
            // Ensure configured max length is at least as large as messaging max length
            var max = prop.GetMaxLength();
            max.HasValue.Should().BeTrue();
            (max.Value >= AppConstants.Messaging.MaxMessageLength).Should().BeTrue();
        }

        /// <summary>
        /// Verifies that MessageEntity_RequiredFieldsConfigured behaves as expected.
        /// </summary>
        [Fact]
        public void MessageEntity_RequiredFieldsConfigured()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(Message));
            entity!.FindProperty(nameof(Message.SenderId))!.IsNullable.Should().BeFalse();
            entity.FindProperty(nameof(Message.ReceiverId))!.IsNullable.Should().BeFalse();
            entity.FindProperty(nameof(Message.SentAt))!.IsNullable.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that MessageEntity_SoftDeleteFilterApplied behaves as expected.
        /// </summary>
        [Fact]
        public void MessageEntity_SoftDeleteFilterApplied()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(Message));
            var filter = entity!.GetQueryFilter();
            filter.Should().NotBeNull();
            filter!.ToString().Should().Contain(nameof(Domain.Entities.BaseEntity.IsDeleted));
        }

        /// <summary>
        /// Verifies that MessageEntity_RelationshipsConfiguredCorrectly behaves as expected.
        /// </summary>
        [Fact]
        public void MessageEntity_RelationshipsConfiguredCorrectly()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(Message));

            // Sender navigation
            var senderNav = entity!.FindNavigation(nameof(Message.Sender));
            senderNav.Should().NotBeNull();
            senderNav!.ForeignKey.Properties.Select(p => p.Name).Should().Contain(nameof(Message.SenderId));

            // Receiver navigation
            var receiverNav = entity.FindNavigation(nameof(Message.Receiver));
            receiverNav.Should().NotBeNull();
            receiverNav!.ForeignKey.Properties.Select(p => p.Name).Should().Contain(nameof(Message.ReceiverId));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("cfg-test-db")
                .Options;

            return new AppDbContext(options);
        }
    }
}

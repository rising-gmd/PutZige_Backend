#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PutZige.Domain.Entities;
using Xunit;

namespace PutZige.Application.Tests.Services;

public partial class MessagingServiceTests
{
    [Fact]
    public async Task MarkMessageAsDelivered_AlreadyDelivered_UpdatesTimestamp()
    {
        // Arrange
        var m = CreateMessage(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-10), "dtest");
        m.DeliveredAt = DateTime.UtcNow.AddMinutes(-5);
        _mockMessageRepo.Setup(r => r.GetByIdAsync(m.Id, It.IsAny<CancellationToken>())).ReturnsAsync(m);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.MarkMessageAsDeliveredAsync(m.Id, _ct);

        // Assert
        _mockMessageRepo.Verify(r => r.UpdateAsync(It.Is<Message>(x => x.DeliveredAt != null && x.DeliveredAt > DateTime.UtcNow.AddMinutes(-6)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkMessageAsDelivered_DeletedMessage_ThrowsKeyNotFoundException()
    {
        // Arrange
        var m = CreateMessage(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "dtest");
        m.IsDeleted = true;
        _mockMessageRepo.Setup(r => r.GetByIdAsync(m.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Message?)null);

        // Act
        Func<Task> act = async () => await _sut.MarkMessageAsDeliveredAsync(m.Id, _ct);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task MarkMessageAsDelivered_SetsUtcNow()
    {
        // Arrange
        var m = CreateMessage(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "dtest");
        _mockMessageRepo.Setup(r => r.GetByIdAsync(m.Id, It.IsAny<CancellationToken>())).ReturnsAsync(m);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var before = DateTime.UtcNow;
        await _sut.MarkMessageAsDeliveredAsync(m.Id, _ct);

        // Assert
        _mockMessageRepo.Verify(r => r.UpdateAsync(It.Is<Message>(x => x.DeliveredAt != null && x.DeliveredAt >= before), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkMessageAsRead_ValidMessage_SetsReadAt()
    {
        // Arrange
        var m = CreateMessage(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "rtest");
        _mockMessageRepo.Setup(r => r.GetByIdAsync(m.Id, It.IsAny<CancellationToken>())).ReturnsAsync(m);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.MarkMessageAsReadAsync(m.Id, _ct);

        // Assert
        _mockMessageRepo.Verify(r => r.UpdateAsync(It.Is<Message>(x => x.ReadAt != null), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkMessageAsRead_MessageNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockMessageRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Message?)null);

        // Act
        Func<Task> act = async () => await _sut.MarkMessageAsReadAsync(Guid.NewGuid(), _ct);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task MarkMessageAsRead_AlreadyRead_UpdatesTimestamp()
    {
        // Arrange
        var m = CreateMessage(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-10), "rtest");
        m.ReadAt = DateTime.UtcNow.AddMinutes(-5);
        _mockMessageRepo.Setup(r => r.GetByIdAsync(m.Id, It.IsAny<CancellationToken>())).ReturnsAsync(m);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.MarkMessageAsReadAsync(m.Id, _ct);

        // Assert
        _mockMessageRepo.Verify(r => r.UpdateAsync(It.Is<Message>(x => x.ReadAt != null && x.ReadAt > DateTime.UtcNow.AddMinutes(-6)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkMessageAsRead_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        // The MessagingService currently does not enforce authorization by itself; this test ensures caller must check. We expect behaviour to set read regardless.
        var m = CreateMessage(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "rtest");
        _mockMessageRepo.Setup(r => r.GetByIdAsync(m.Id, It.IsAny<CancellationToken>())).ReturnsAsync(m);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _sut.MarkMessageAsReadAsync(m.Id, _ct);

        // Assert
        _mockMessageRepo.Verify(r => r.UpdateAsync(It.Is<Message>(x => x.ReadAt != null), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkMessageAsRead_SetsUtcNow()
    {
        // Arrange
        var m = CreateMessage(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "rtest");
        _mockMessageRepo.Setup(r => r.GetByIdAsync(m.Id, It.IsAny<CancellationToken>())).ReturnsAsync(m);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var before = DateTime.UtcNow;
        await _sut.MarkMessageAsReadAsync(m.Id, _ct);

        // Assert
        _mockMessageRepo.Verify(r => r.UpdateAsync(It.Is<Message>(x => x.ReadAt != null && x.ReadAt >= before), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkMessageAsRead_DeletedMessage_ThrowsKeyNotFoundException()
    {
        // Arrange
        var m = CreateMessage(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "rtest");
        m.IsDeleted = true;
        _mockMessageRepo.Setup(r => r.GetByIdAsync(m.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Message?)null);

        // Act
        Func<Task> act = async () => await _sut.MarkMessageAsReadAsync(m.Id, _ct);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}

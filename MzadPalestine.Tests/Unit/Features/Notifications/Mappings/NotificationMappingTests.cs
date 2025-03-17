using FluentAssertions;
using Mapster;
using MzadPalestine.Application.Common.Mappings;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Core.Entities;
using Xunit;

namespace MzadPalestine.Tests.Unit.Features.Notifications.Mappings;

public class NotificationMappingTests
{
    private readonly TypeAdapterConfig _config;

    public NotificationMappingTests()
    {
        _config = new TypeAdapterConfig();
        var mappingConfig = new NotificationMappingConfig();
        mappingConfig.Register(_config);
    }

    [Fact]
    public void NotificationToNotificationDto_ShouldMapAllProperties()
    {
        // Arrange
        var notification = new Notification
        {
            Id = 1,
            Title = "Test Notification",
            Message = "Test Message",
            Type = NotificationType.AuctionCreated,
            IsRead = false,
            ReadAt = null,
            ActionUrl = "/test/url",
            ImageUrl = "/test/image.jpg",
            CreatedAt = DateTime.UtcNow,
            UserId = 1
        };

        // Act
        var dto = notification.Adapt<NotificationDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(notification.Id);
        dto.Title.Should().Be(notification.Title);
        dto.Message.Should().Be(notification.Message);
        dto.Type.Should().Be(notification.Type);
        dto.IsRead.Should().Be(notification.IsRead);
        dto.ReadAt.Should().Be(notification.ReadAt);
        dto.ActionUrl.Should().Be(notification.ActionUrl);
        dto.ImageUrl.Should().Be(notification.ImageUrl);
        dto.CreatedAt.Should().Be(notification.CreatedAt);
    }

    [Fact]
    public void NotificationToNotificationDto_ShouldHandleNullValues()
    {
        // Arrange
        var notification = new Notification
        {
            Id = 1,
            Title = "Test Notification",
            Message = "Test Message",
            Type = NotificationType.AuctionCreated,
            IsRead = false,
            ReadAt = null,
            ActionUrl = null,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UserId = 1
        };

        // Act
        var dto = notification.Adapt<NotificationDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.ActionUrl.Should().BeNull();
        dto.ImageUrl.Should().BeNull();
        dto.ReadAt.Should().BeNull();
    }

    [Fact]
    public void NotificationToNotificationDto_ShouldMapReadAtWhenRead()
    {
        // Arrange
        var readAt = DateTime.UtcNow;
        var notification = new Notification
        {
            Id = 1,
            Title = "Test Notification",
            Message = "Test Message",
            Type = NotificationType.AuctionCreated,
            IsRead = true,
            ReadAt = readAt,
            CreatedAt = DateTime.UtcNow,
            UserId = 1
        };

        // Act
        var dto = notification.Adapt<NotificationDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.IsRead.Should().BeTrue();
        dto.ReadAt.Should().Be(readAt);
    }

    [Theory]
    [InlineData(NotificationType.AuctionCreated)]
    [InlineData(NotificationType.AuctionEnded)]
    [InlineData(NotificationType.AuctionCancelled)]
    [InlineData(NotificationType.BidPlaced)]
    [InlineData(NotificationType.OutBid)]
    [InlineData(NotificationType.TransactionCreated)]
    public void NotificationToNotificationDto_ShouldMapAllNotificationTypes(NotificationType type)
    {
        // Arrange
        var notification = new Notification
        {
            Id = 1,
            Title = "Test Notification",
            Message = "Test Message",
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UserId = 1
        };

        // Act
        var dto = notification.Adapt<NotificationDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.Type.Should().Be(type);
    }

    [Fact]
    public void NotificationToNotificationDto_ShouldPreserveTimestampPrecision()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var readAt = DateTime.UtcNow.AddMinutes(5);
        var notification = new Notification
        {
            Id = 1,
            Title = "Test Notification",
            Message = "Test Message",
            Type = NotificationType.AuctionCreated,
            IsRead = true,
            ReadAt = readAt,
            CreatedAt = createdAt,
            UserId = 1
        };

        // Act
        var dto = notification.Adapt<NotificationDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.CreatedAt.Should().Be(createdAt);
        dto.ReadAt.Should().Be(readAt);
    }

    [Fact]
    public void NotificationToNotificationDto_ShouldMapListOfNotifications()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            new()
            {
                Id = 1,
                Title = "Test Notification 1",
                Message = "Test Message 1",
                Type = NotificationType.AuctionCreated,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UserId = 1
            },
            new()
            {
                Id = 2,
                Title = "Test Notification 2",
                Message = "Test Message 2",
                Type = NotificationType.BidPlaced,
                IsRead = true,
                ReadAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UserId = 1
            }
        };

        // Act
        var dtos = notifications.Adapt<List<NotificationDto>>(_config);

        // Assert
        dtos.Should().NotBeNull();
        dtos.Should().HaveCount(2);
        dtos.Should().BeInAscendingOrder(dto => dto.Id);
        dtos.Select(dto => dto.Title).Should().BeEquivalentTo(notifications.Select(n => n.Title));
    }
}

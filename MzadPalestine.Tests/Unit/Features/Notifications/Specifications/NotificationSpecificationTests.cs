using FluentAssertions;
using MzadPalestine.Application.Features.Notifications.Specifications;
using MzadPalestine.Core.Entities;
using Xunit;

namespace MzadPalestine.Tests.Unit.Features.Notifications.Specifications;

public class NotificationSpecificationTests
{
    [Fact]
    public void GetUserNotificationsSpecification_ShouldFilterByUserId()
    {
        // Arrange
        const int userId = 1;
        var spec = new GetUserNotificationsSpecification(userId);
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId },
            new() { Id = 2, UserId = userId + 1 }
        }.AsQueryable();

        // Act
        var filteredNotifications = notifications.Where(spec.Criteria.Compile());

        // Assert
        filteredNotifications.Should().HaveCount(1);
        filteredNotifications.First().UserId.Should().Be(userId);
    }

    [Fact]
    public void GetUserNotificationsSpecification_ShouldFilterByReadStatus()
    {
        // Arrange
        const int userId = 1;
        const bool isRead = true;
        var spec = new GetUserNotificationsSpecification(userId, isRead);
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, IsRead = true },
            new() { Id = 2, UserId = userId, IsRead = false }
        }.AsQueryable();

        // Act
        var filteredNotifications = notifications.Where(spec.Criteria.Compile());

        // Assert
        filteredNotifications.Should().HaveCount(1);
        filteredNotifications.First().IsRead.Should().BeTrue();
    }

    [Fact]
    public void GetUserNotificationsSpecification_ShouldFilterBySearchTerm()
    {
        // Arrange
        const int userId = 1;
        const string searchTerm = "test";
        var spec = new GetUserNotificationsSpecification(userId, null, searchTerm);
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, Title = "Test Notification", Message = "Content" },
            new() { Id = 2, UserId = userId, Title = "Other", Message = "Test Content" },
            new() { Id = 3, UserId = userId, Title = "Other", Message = "Content" }
        }.AsQueryable();

        // Act
        var filteredNotifications = notifications.Where(spec.Criteria.Compile());

        // Assert
        filteredNotifications.Should().HaveCount(2);
        filteredNotifications.All(n => 
            n.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
            n.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }

    [Fact]
    public void GetUserNotificationsSpecification_ShouldSortByCreatedAt()
    {
        // Arrange
        const int userId = 1;
        var spec = new GetUserNotificationsSpecification(userId, null, null, "createdat", true);
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 2, UserId = userId, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = 3, UserId = userId, CreatedAt = DateTime.UtcNow }
        }.AsQueryable();

        // Act
        var orderedNotifications = spec.OrderBy != null 
            ? notifications.OrderByDescending(spec.OrderBy.Compile())
            : notifications;

        // Assert
        orderedNotifications.Should().BeInDescendingOrder(n => n.CreatedAt);
    }

    [Fact]
    public void GetUserNotificationsSpecification_ShouldSortByReadAt()
    {
        // Arrange
        const int userId = 1;
        var spec = new GetUserNotificationsSpecification(userId, null, null, "readat", false);
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, ReadAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 2, UserId = userId, ReadAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = 3, UserId = userId, ReadAt = DateTime.UtcNow }
        }.AsQueryable();

        // Act
        var orderedNotifications = spec.OrderBy != null 
            ? notifications.OrderBy(spec.OrderBy.Compile())
            : notifications;

        // Assert
        orderedNotifications.Should().BeInAscendingOrder(n => n.ReadAt);
    }

    [Fact]
    public void GetUnreadNotificationsSpecification_ShouldFilterUnreadNotifications()
    {
        // Arrange
        const int userId = 1;
        var spec = new GetUnreadNotificationsSpecification(userId);
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, IsRead = false },
            new() { Id = 2, UserId = userId, IsRead = true },
            new() { Id = 3, UserId = userId + 1, IsRead = false }
        }.AsQueryable();

        // Act
        var filteredNotifications = notifications.Where(spec.Criteria.Compile());

        // Assert
        filteredNotifications.Should().HaveCount(1);
        filteredNotifications.First().UserId.Should().Be(userId);
        filteredNotifications.First().IsRead.Should().BeFalse();
    }

    [Fact]
    public void GetNotificationByIdSpecification_ShouldFilterById()
    {
        // Arrange
        const int notificationId = 1;
        var spec = new GetNotificationByIdSpecification(notificationId);
        var notifications = new List<Notification>
        {
            new() { Id = notificationId, UserId = 1 },
            new() { Id = notificationId + 1, UserId = 1 }
        }.AsQueryable();

        // Act
        var filteredNotifications = notifications.Where(spec.Criteria.Compile());

        // Assert
        filteredNotifications.Should().HaveCount(1);
        filteredNotifications.First().Id.Should().Be(notificationId);
    }
}

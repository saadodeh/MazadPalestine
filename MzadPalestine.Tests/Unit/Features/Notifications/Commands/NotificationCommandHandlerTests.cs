using FluentAssertions;
using Moq;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteAllNotifications;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteNotification;
using MzadPalestine.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using MzadPalestine.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Unit.Features.Notifications.Commands;

public class NotificationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationRepositoryMock;

    public NotificationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _identityServiceMock = new Mock<IIdentityService>();
        _notificationRepositoryMock = new Mock<IGenericRepository<Notification>>();

        _unitOfWorkMock.Setup(x => x.Repository<Notification>())
            .Returns(_notificationRepositoryMock.Object);
    }

    [Fact]
    public async Task DeleteNotification_Success_WhenNotificationExists()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        var notification = new Notification { Id = 1, UserId = currentUser.Id };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notification.Id))
            .ReturnsAsync(notification);

        var handler = new DeleteNotificationCommandHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var command = new DeleteNotificationCommand(notification.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notificationRepositoryMock.Verify(x => x.Delete(notification), Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAllNotifications_Success_WhenUserHasNotifications()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = currentUser.Id },
            new() { Id = 2, UserId = currentUser.Id }
        };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetQueryable())
            .Returns(notifications.AsQueryable());

        var handler = new DeleteAllNotificationsCommandHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var command = new DeleteAllNotificationsCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(notifications.Count);
        _notificationRepositoryMock.Verify(x => x.Delete(It.IsAny<Notification>()), Times.Exactly(notifications.Count));
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationAsRead_Success_WhenNotificationExists()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        var notification = new Notification { Id = 1, UserId = currentUser.Id, IsRead = false };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notification.Id))
            .ReturnsAsync(notification);

        var handler = new MarkNotificationAsReadCommandHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var command = new MarkNotificationAsReadCommand(notification.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkAllNotificationsAsRead_Success_WhenUserHasUnreadNotifications()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = currentUser.Id, IsRead = false },
            new() { Id = 2, UserId = currentUser.Id, IsRead = false }
        };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetQueryable())
            .Returns(notifications.AsQueryable());

        var handler = new MarkAllNotificationsAsReadCommandHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var command = new MarkAllNotificationsAsReadCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(notifications.Count);
        notifications.All(n => n.IsRead && n.ReadAt != null).Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteNotification_Failure_WhenNotificationDoesNotExist()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        const int notificationId = 1;

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification?)null);

        var handler = new DeleteNotificationCommandHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var command = new DeleteNotificationCommand(notificationId);

        // Act & Assert
        await Assert.ThrowsAsync<Application.Common.Exceptions.NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteNotification_Failure_WhenUserDoesNotOwnNotification()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        var notification = new Notification { Id = 1, UserId = 2 }; // Different user ID

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notification.Id))
            .ReturnsAsync(notification);

        var handler = new DeleteNotificationCommandHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var command = new DeleteNotificationCommand(notification.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You can only delete your own notifications");
    }
}

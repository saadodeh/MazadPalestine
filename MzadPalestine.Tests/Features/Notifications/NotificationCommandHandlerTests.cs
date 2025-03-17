using Moq;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteNotification;
using MzadPalestine.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Features.Notifications;

public class NotificationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<IRepository<Notification>> _mockNotificationRepo;
    private readonly User _currentUser;

    public NotificationCommandHandlerTests()
    {
        _mockNotificationRepo = new Mock<IRepository<Notification>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockIdentityService = new Mock<IIdentityService>();
        
        _currentUser = new User { Id = 1, UserName = "testuser" };
        _mockIdentityService.Setup(s => s.GetCurrentUserAsync())
            .ReturnsAsync(_currentUser);
        
        _mockUnitOfWork.Setup(uow => uow.Repository<Notification>())
            .Returns(_mockNotificationRepo.Object);
    }

    [Fact]
    public async Task DeleteNotification_WhenUserOwnsNotification_ShouldSucceed()
    {
        // Arrange
        var handler = new DeleteNotificationCommandHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notification = new Notification { Id = 1, UserId = _currentUser.Id };
        
        _mockNotificationRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(notification);

        // Act
        var result = await handler.Handle(new DeleteNotificationCommand(1), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mockNotificationRepo.Verify(r => r.Delete(notification), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteNotification_WhenUserDoesNotOwnNotification_ShouldFail()
    {
        // Arrange
        var handler = new DeleteNotificationCommandHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notification = new Notification { Id = 1, UserId = _currentUser.Id + 1 };
        
        _mockNotificationRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(notification);

        // Act
        var result = await handler.Handle(new DeleteNotificationCommand(1), CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("own notifications", result.Error);
        _mockNotificationRepo.Verify(r => r.Delete(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task MarkNotificationAsRead_WhenUserOwnsNotification_ShouldSucceed()
    {
        // Arrange
        var handler = new MarkNotificationAsReadCommandHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notification = new Notification { Id = 1, UserId = _currentUser.Id, IsRead = false };
        
        _mockNotificationRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(notification);

        // Act
        var result = await handler.Handle(new MarkNotificationAsReadCommand(1), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
        _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationAsRead_WhenUserDoesNotOwnNotification_ShouldFail()
    {
        // Arrange
        var handler = new MarkNotificationAsReadCommandHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notification = new Notification { Id = 1, UserId = _currentUser.Id + 1, IsRead = false };
        
        _mockNotificationRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(notification);

        // Act
        var result = await handler.Handle(new MarkNotificationAsReadCommand(1), CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("own notifications", result.Error);
        Assert.False(notification.IsRead);
        Assert.Null(notification.ReadAt);
    }

    [Fact]
    public async Task MarkAllNotificationsAsRead_ShouldSucceed()
    {
        // Arrange
        var handler = new MarkAllNotificationsAsReadCommandHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = _currentUser.Id, IsRead = false },
            new() { Id = 2, UserId = _currentUser.Id, IsRead = false }
        };

        _mockNotificationRepo.Setup(r => r.FindAsync(It.IsAny<ISpecification<Notification>>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await handler.Handle(new MarkAllNotificationsAsReadCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.All(notifications, n =>
        {
            Assert.True(n.IsRead);
            Assert.NotNull(n.ReadAt);
        });
        _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAllNotifications_ShouldSucceed()
    {
        // Arrange
        var handler = new DeleteAllNotificationsCommandHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = _currentUser.Id },
            new() { Id = 2, UserId = _currentUser.Id }
        };

        _mockNotificationRepo.Setup(r => r.FindAsync(It.IsAny<ISpecification<Notification>>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await handler.Handle(new DeleteAllNotificationsCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mockNotificationRepo.Verify(r => r.DeleteRange(notifications), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
    }
}

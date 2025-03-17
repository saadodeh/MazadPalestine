using Moq;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;
using MzadPalestine.Application.Features.Notifications.Queries.GetUserNotifications;
using MzadPalestine.Application.Features.Notifications.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Features.Notifications;

public class NotificationQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<IRepository<Notification>> _mockNotificationRepo;
    private readonly User _currentUser;

    public NotificationQueryHandlerTests()
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
    public async Task GetUserNotifications_ShouldReturnPaginatedList()
    {
        // Arrange
        var handler = new GetUserNotificationsQueryHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notifications = new List<Notification>
        {
            new()
            {
                Id = 1,
                UserId = _currentUser.Id,
                Title = "Test Notification 1",
                Message = "Test Message 1",
                Type = NotificationType.AuctionCreated,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                UserId = _currentUser.Id,
                Title = "Test Notification 2",
                Message = "Test Message 2",
                Type = NotificationType.BidPlaced,
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _mockNotificationRepo.Setup(r => r.FindAsync(It.IsAny<GetUserNotificationsSpecification>()))
            .ReturnsAsync(notifications);
        
        _mockNotificationRepo.Setup(r => r.CountAsync(It.IsAny<GetUserNotificationsSpecification>()))
            .ReturnsAsync(notifications.Count);

        var query = new GetUserNotificationsQuery(PageNumber: 1, PageSize: 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count());
        Assert.Equal(2, result.Data.TotalCount);
        Assert.Equal(1, result.Data.CurrentPage);
        Assert.Equal(1, result.Data.TotalPages);
    }

    [Fact]
    public async Task GetUserNotifications_WithFilters_ShouldReturnFilteredList()
    {
        // Arrange
        var handler = new GetUserNotificationsQueryHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notifications = new List<Notification>
        {
            new()
            {
                Id = 1,
                UserId = _currentUser.Id,
                Title = "Test Notification",
                Message = "Test Message",
                Type = NotificationType.AuctionCreated,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockNotificationRepo.Setup(r => r.FindAsync(It.IsAny<GetUserNotificationsSpecification>()))
            .ReturnsAsync(notifications);
        
        _mockNotificationRepo.Setup(r => r.CountAsync(It.IsAny<GetUserNotificationsSpecification>()))
            .ReturnsAsync(notifications.Count);

        var query = new GetUserNotificationsQuery(
            PageNumber: 1,
            PageSize: 10,
            IsRead: false,
            SearchTerm: "Test",
            SortBy: "createdat",
            SortDescending: true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("Test Notification", result.Data.Items.First().Title);
        Assert.False(result.Data.Items.First().IsRead);
    }

    [Fact]
    public async Task GetUnreadNotificationsCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var handler = new GetUnreadNotificationsCountQueryHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var notifications = new List<Notification>
        {
            new() { UserId = _currentUser.Id, IsRead = false },
            new() { UserId = _currentUser.Id, IsRead = false },
            new() { UserId = _currentUser.Id, IsRead = true }
        };

        _mockNotificationRepo.Setup(r => r.CountAsync(It.IsAny<ISpecification<Notification>>()))
            .ReturnsAsync(notifications.Count(n => !n.IsRead));

        // Act
        var result = await handler.Handle(new GetUnreadNotificationsCountQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Data);
    }

    [Fact]
    public async Task GetUserNotifications_WithInvalidPage_ShouldReturnError()
    {
        // Arrange
        var handler = new GetUserNotificationsQueryHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var query = new GetUserNotificationsQuery(PageNumber: 0, PageSize: 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Page number", result.Error);
    }

    [Fact]
    public async Task GetUserNotifications_WithInvalidPageSize_ShouldReturnError()
    {
        // Arrange
        var handler = new GetUserNotificationsQueryHandler(_mockUnitOfWork.Object, _mockIdentityService.Object);
        var query = new GetUserNotificationsQuery(PageNumber: 1, PageSize: 0);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Page size", result.Error);
    }
}

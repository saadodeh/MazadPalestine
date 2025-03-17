using FluentAssertions;
using Mapster;
using Moq;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;
using MzadPalestine.Application.Features.Notifications.Queries.GetUserNotifications;
using MzadPalestine.Application.Features.Notifications.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Unit.Features.Notifications.Queries;

public class NotificationQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationRepositoryMock;
    private readonly TypeAdapterConfig _mapperConfig;

    public NotificationQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _identityServiceMock = new Mock<IIdentityService>();
        _notificationRepositoryMock = new Mock<IGenericRepository<Notification>>();
        _mapperConfig = new TypeAdapterConfig();

        _unitOfWorkMock.Setup(x => x.Repository<Notification>())
            .Returns(_notificationRepositoryMock.Object);

        // Configure Mapster
        _mapperConfig.NewConfig<Notification, NotificationDto>();
        TypeAdapterConfig.GlobalSettings = _mapperConfig;
    }

    [Fact]
    public async Task GetUserNotifications_Success_ReturnsCorrectPaginatedList()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = currentUser.Id, Title = "Test 1", Message = "Message 1", IsRead = false },
            new() { Id = 2, UserId = currentUser.Id, Title = "Test 2", Message = "Message 2", IsRead = true }
        };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<GetUserNotificationsSpecification>()))
            .ReturnsAsync(notifications.Count);

        _notificationRepositoryMock.Setup(x => x.ListAsync(It.IsAny<GetUserNotificationsSpecification>()))
            .ReturnsAsync(notifications);

        var handler = new GetUserNotificationsQueryHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var query = new GetUserNotificationsQuery(1, 10, null, null, null, true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        result.Data.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetUserNotifications_Success_AppliesFilters()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = currentUser.Id, Title = "Test 1", Message = "Message 1", IsRead = false }
        };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<GetUserNotificationsSpecification>()))
            .ReturnsAsync(notifications.Count);

        _notificationRepositoryMock.Setup(x => x.ListAsync(It.IsAny<GetUserNotificationsSpecification>()))
            .ReturnsAsync(notifications);

        var handler = new GetUserNotificationsQueryHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var query = new GetUserNotificationsQuery(1, 10, false, "Test", "createdat", true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().IsRead.Should().BeFalse();
        result.Data.Items.First().Title.Should().Contain("Test");
    }

    [Fact]
    public async Task GetUnreadNotificationsCount_Success_ReturnsCorrectCount()
    {
        // Arrange
        var currentUser = new User { Id = 1 };
        var unreadCount = 5;

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<GetUnreadNotificationsSpecification>()))
            .ReturnsAsync(unreadCount);

        var handler = new GetUnreadNotificationsCountQueryHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var query = new GetUnreadNotificationsCountQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(unreadCount);
    }

    [Fact]
    public async Task GetUserNotifications_Failure_WhenUserNotFound()
    {
        // Arrange
        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync((User?)null);

        var handler = new GetUserNotificationsQueryHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var query = new GetUserNotificationsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task GetUnreadNotificationsCount_Failure_WhenUserNotFound()
    {
        // Arrange
        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync((User?)null);

        var handler = new GetUnreadNotificationsCountQueryHandler(_unitOfWorkMock.Object, _identityServiceMock.Object);
        var query = new GetUnreadNotificationsCountQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }
}

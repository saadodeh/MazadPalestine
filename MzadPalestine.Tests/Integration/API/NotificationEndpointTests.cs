using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MzadPalestine.API;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Integration.API;

public class NotificationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationRepositoryMock;

    public NotificationEndpointTests(WebApplicationFactory<Program> factory)
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationRepositoryMock = new Mock<IGenericRepository<Notification>>();

        _unitOfWorkMock.Setup(x => x.Repository<Notification>())
            .Returns(_notificationRepositoryMock.Object);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_identityServiceMock.Object);
                services.AddSingleton(_unitOfWorkMock.Object);
            });
        });
    }

    [Fact]
    public async Task GetNotifications_ReturnsOkWithPaginatedList_WhenUserIsAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var currentUser = new User { Id = 1 };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = currentUser.Id, Title = "Test 1" },
            new() { Id = 2, UserId = currentUser.Id, Title = "Test 2" }
        };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.ListAsync(It.IsAny<ISpecification<Notification>>()))
            .ReturnsAsync(notifications);

        // Act
        var response = await client.GetAsync("/api/notifications?pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<Result<PaginatedList<NotificationDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsOkWithCount_WhenUserIsAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var currentUser = new User { Id = 1 };
        const int unreadCount = 5;

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.CountAsync(It.IsAny<ISpecification<Notification>>()))
            .ReturnsAsync(unreadCount);

        // Act
        var response = await client.GetAsync("/api/notifications/unread/count");
        var result = await response.Content.ReadFromJsonAsync<Result<int>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(unreadCount);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsOk_WhenNotificationExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var currentUser = new User { Id = 1 };
        var notification = new Notification { Id = 1, UserId = currentUser.Id };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notification.Id))
            .ReturnsAsync(notification);

        // Act
        var response = await client.PutAsync($"/api/notifications/{notification.Id}/read", null);
        var result = await response.Content.ReadFromJsonAsync<Result<Unit>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsOkWithCount_WhenUserHasUnreadNotifications()
    {
        // Arrange
        var client = _factory.CreateClient();
        var currentUser = new User { Id = 1 };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = currentUser.Id, IsRead = false },
            new() { Id = 2, UserId = currentUser.Id, IsRead = false }
        };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.ListAsync(It.IsAny<ISpecification<Notification>>()))
            .ReturnsAsync(notifications);

        // Act
        var response = await client.PutAsync("/api/notifications/read/all", null);
        var result = await response.Content.ReadFromJsonAsync<Result<int>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(notifications.Count);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenNotificationExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var currentUser = new User { Id = 1 };
        var notification = new Notification { Id = 1, UserId = currentUser.Id };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notification.Id))
            .ReturnsAsync(notification);

        // Act
        var response = await client.DeleteAsync($"/api/notifications/{notification.Id}");
        var result = await response.Content.ReadFromJsonAsync<Result<Unit>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAll_ReturnsOkWithCount_WhenUserHasNotifications()
    {
        // Arrange
        var client = _factory.CreateClient();
        var currentUser = new User { Id = 1 };
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = currentUser.Id },
            new() { Id = 2, UserId = currentUser.Id }
        };

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.ListAsync(It.IsAny<ISpecification<Notification>>()))
            .ReturnsAsync(notifications);

        // Act
        var response = await client.DeleteAsync("/api/notifications");
        var result = await response.Content.ReadFromJsonAsync<Result<int>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(notifications.Count);
    }

    [Fact]
    public async Task GetNotifications_ReturnsBadRequest_WhenUserIsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync((User?)null);

        // Act
        var response = await client.GetAsync("/api/notifications");
        var result = await response.Content.ReadFromJsonAsync<Result<PaginatedList<NotificationDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenNotificationDoesNotBelongToUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var currentUser = new User { Id = 1 };
        var notification = new Notification { Id = 1, UserId = 2 }; // Different user ID

        _identityServiceMock.Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notification.Id))
            .ReturnsAsync(notification);

        // Act
        var response = await client.DeleteAsync($"/api/notifications/{notification.Id}");
        var result = await response.Content.ReadFromJsonAsync<Result<Unit>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You can only delete your own notifications");
    }
}

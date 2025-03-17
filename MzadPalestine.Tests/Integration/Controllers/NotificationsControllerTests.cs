using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MzadPalestine.API.Controllers;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Notifications;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteAllNotifications;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteNotification;
using MzadPalestine.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using MzadPalestine.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using MzadPalestine.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;
using MzadPalestine.Application.Features.Notifications.Queries.GetUserNotifications;
using Xunit;

namespace MzadPalestine.Tests.Integration.Controllers;

public class NotificationsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NotificationsController _controller;

    public NotificationsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new NotificationsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetNotifications_ReturnsOkResult_WhenRequestIsValid()
    {
        // Arrange
        var expectedResult = Result<PaginatedList<NotificationDto>>.Success(
            new PaginatedList<NotificationDto>(new List<NotificationDto>(), 0, 1, 10));

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserNotificationsQuery>(), default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetNotifications();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsOkResult_WhenRequestIsValid()
    {
        // Arrange
        var expectedResult = Result<int>.Success(5);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUnreadNotificationsCountQuery>(), default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetUnreadCount();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsOkResult_WhenRequestIsValid()
    {
        // Arrange
        var expectedResult = Result<Unit>.Success(Unit.Value);

        _mediatorMock.Setup(m => m.Send(It.IsAny<MarkNotificationAsReadCommand>(), default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.MarkAsRead(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsOkResult_WhenRequestIsValid()
    {
        // Arrange
        var expectedResult = Result<int>.Success(10);

        _mediatorMock.Setup(m => m.Send(It.IsAny<MarkAllNotificationsAsReadCommand>(), default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.MarkAllAsRead();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task Delete_ReturnsOkResult_WhenRequestIsValid()
    {
        // Arrange
        var expectedResult = Result<Unit>.Success(Unit.Value);

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteNotificationCommand>(), default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task DeleteAll_ReturnsOkResult_WhenRequestIsValid()
    {
        // Arrange
        var expectedResult = Result<int>.Success(15);

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllNotificationsCommand>(), default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.DeleteAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetNotifications_ReturnsBadRequest_WhenRequestFails()
    {
        // Arrange
        var expectedResult = Result<PaginatedList<NotificationDto>>.Failure("Error message");

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserNotificationsQuery>(), default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetNotifications();

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(expectedResult);
    }
}

using FluentValidation.TestHelper;
using Moq;
using MzadPalestine.Application.Features.Notifications.Commands.DeleteNotification;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Unit.Features.Notifications.Validators;

public class NotificationValidatorTests
{
    private readonly Mock<IGenericRepository<Notification>> _notificationRepositoryMock;
    private readonly DeleteNotificationCommandValidator _validator;

    public NotificationValidatorTests()
    {
        _notificationRepositoryMock = new Mock<IGenericRepository<Notification>>();
        _validator = new DeleteNotificationCommandValidator(_notificationRepositoryMock.Object);
    }

    [Fact]
    public async Task Validator_ShouldPass_WhenNotificationExists()
    {
        // Arrange
        var command = new DeleteNotificationCommand(1);
        var notification = new Notification { Id = 1 };

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(notification);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_ShouldFail_WhenNotificationDoesNotExist()
    {
        // Arrange
        var command = new DeleteNotificationCommand(1);

        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Notification?)null);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Notification not found");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_ShouldFail_WhenNotificationIdIsInvalid(int id)
    {
        // Arrange
        var command = new DeleteNotificationCommand(id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}

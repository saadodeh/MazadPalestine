using Moq;
using MzadPalestine.Application.Features.Notifications.EventHandlers;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Features.Notifications;

public class NotificationEventHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Notification>> _mockNotificationRepo;

    public NotificationEventHandlerTests()
    {
        _mockNotificationRepo = new Mock<IRepository<Notification>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(uow => uow.Repository<Notification>()).Returns(_mockNotificationRepo.Object);
    }

    [Fact]
    public async Task AuctionCreatedEventHandler_ShouldCreateNotification()
    {
        // Arrange
        var handler = new AuctionCreatedEventHandler(_mockUnitOfWork.Object);
        var @event = new AuctionCreatedEvent
        {
            AuctionId = 1,
            Title = "Test Auction",
            SellerId = 1,
            StartingPrice = 100
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _mockNotificationRepo.Verify(
            r => r.AddAsync(It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Title == "Auction Created Successfully" &&
                n.Message.Contains(@event.Title) &&
                n.Message.Contains(@event.StartingPrice.ToString())
            )),
            Times.Once
        );
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AuctionEndedEventHandler_WithWinner_ShouldCreateTwoNotifications()
    {
        // Arrange
        var handler = new AuctionEndedEventHandler(_mockUnitOfWork.Object);
        var @event = new AuctionEndedEvent
        {
            AuctionId = 1,
            Title = "Test Auction",
            SellerId = 1,
            WinnerId = 2,
            FinalPrice = 150
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _mockNotificationRepo.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 2 &&
                notifications.Any(n => n.UserId == @event.SellerId) &&
                notifications.Any(n => n.UserId == @event.WinnerId)
            )),
            Times.Once
        );
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task BidPlacedEventHandler_WithPreviousBidder_ShouldCreateTwoNotifications()
    {
        // Arrange
        var handler = new BidPlacedEventHandler(_mockUnitOfWork.Object);
        var @event = new BidPlacedEvent
        {
            AuctionId = 1,
            Title = "Test Auction",
            BidderId = 2,
            SellerId = 1,
            PreviousBidderId = 3,
            Amount = 200
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _mockNotificationRepo.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 2 &&
                notifications.Any(n => n.UserId == @event.SellerId) &&
                notifications.Any(n => n.UserId == @event.PreviousBidderId)
            )),
            Times.Once
        );
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionCreatedEventHandler_ShouldCreateTwoNotifications()
    {
        // Arrange
        var handler = new TransactionCreatedEventHandler(_mockUnitOfWork.Object);
        var @event = new TransactionCreatedEvent
        {
            TransactionId = 1,
            AuctionId = 1,
            Title = "Test Auction",
            BuyerId = 2,
            SellerId = 1,
            Amount = 200,
            DueDate = DateTime.UtcNow.AddDays(3)
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _mockNotificationRepo.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 2 &&
                notifications.Any(n => n.UserId == @event.BuyerId && n.Type == Core.Enums.NotificationType.PaymentRequired) &&
                notifications.Any(n => n.UserId == @event.SellerId && n.Type == Core.Enums.NotificationType.TransactionCreated)
            )),
            Times.Once
        );
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionCompletedEventHandler_ShouldCreateTwoNotifications()
    {
        // Arrange
        var handler = new TransactionCompletedEventHandler(_mockUnitOfWork.Object);
        var @event = new TransactionCompletedEvent
        {
            TransactionId = 1,
            AuctionId = 1,
            Title = "Test Auction",
            BuyerId = 2,
            SellerId = 1,
            Amount = 200,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _mockNotificationRepo.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 2 &&
                notifications.Any(n => n.UserId == @event.BuyerId && n.Type == Core.Enums.NotificationType.PaymentSent) &&
                notifications.Any(n => n.UserId == @event.SellerId && n.Type == Core.Enums.NotificationType.PaymentReceived)
            )),
            Times.Once
        );
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }
}

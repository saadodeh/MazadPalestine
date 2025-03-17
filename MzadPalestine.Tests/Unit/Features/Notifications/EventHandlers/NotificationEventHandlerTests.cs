using Moq;
using MzadPalestine.Application.Features.Notifications.EventHandlers;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Unit.Features.Notifications.EventHandlers;

public class NotificationEventHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Notification>> _notificationRepositoryMock;

    public NotificationEventHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationRepositoryMock = new Mock<IGenericRepository<Notification>>();

        _unitOfWorkMock.Setup(x => x.Repository<Notification>())
            .Returns(_notificationRepositoryMock.Object);
    }

    [Fact]
    public async Task AuctionCreatedEventHandler_ShouldCreateNotification_ForSeller()
    {
        // Arrange
        var handler = new AuctionCreatedEventHandler(_unitOfWorkMock.Object);
        var @event = new AuctionCreatedEvent
        {
            AuctionId = 1,
            Title = "Test Auction",
            SellerId = 1
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Type == NotificationType.AuctionCreated &&
                n.Title.Contains(@event.Title) &&
                n.ActionUrl.Contains(@event.AuctionId.ToString())
            )), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AuctionEndedEventHandler_ShouldCreateNotifications_ForSellerAndWinner()
    {
        // Arrange
        var handler = new AuctionEndedEventHandler(_unitOfWorkMock.Object);
        var @event = new AuctionEndedEvent
        {
            AuctionId = 1,
            Title = "Test Auction",
            SellerId = 1,
            WinnerId = 2,
            FinalPrice = 100
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Type == NotificationType.AuctionEnded &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.FinalPrice.ToString())
            )), Times.Once);

        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.WinnerId &&
                n.Type == NotificationType.AuctionWon &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.FinalPrice.ToString())
            )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AuctionCancelledEventHandler_ShouldCreateNotifications_ForSellerAndBidders()
    {
        // Arrange
        var handler = new AuctionCancelledEventHandler(_unitOfWorkMock.Object);
        var @event = new AuctionCancelledEvent
        {
            AuctionId = 1,
            Title = "Test Auction",
            SellerId = 1,
            BidderIds = new List<int> { 2, 3, 4 }
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Type == NotificationType.AuctionCancelled &&
                n.Title.Contains(@event.Title)
            )), Times.Once);

        foreach (var bidderId in @event.BidderIds)
        {
            _notificationRepositoryMock.Verify(x => x.AddAsync(
                It.Is<Notification>(n =>
                    n.UserId == bidderId &&
                    n.Type == NotificationType.AuctionCancelled &&
                    n.Title.Contains(@event.Title)
                )), Times.Once);
        }

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task BidPlacedEventHandler_ShouldCreateNotifications_ForSellerAndPreviousBidder()
    {
        // Arrange
        var handler = new BidPlacedEventHandler(_unitOfWorkMock.Object);
        var @event = new BidPlacedEvent
        {
            AuctionId = 1,
            Title = "Test Auction",
            BidderId = 2,
            SellerId = 1,
            PreviousBidderId = 3,
            Amount = 100
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Type == NotificationType.BidPlaced &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.Amount.ToString())
            )), Times.Once);

        if (@event.PreviousBidderId.HasValue)
        {
            _notificationRepositoryMock.Verify(x => x.AddAsync(
                It.Is<Notification>(n =>
                    n.UserId == @event.PreviousBidderId &&
                    n.Type == NotificationType.OutBid &&
                    n.Title.Contains(@event.Title) &&
                    n.Message.Contains(@event.Amount.ToString())
                )), Times.Once);
        }

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionCreatedEventHandler_ShouldCreateNotifications_ForBuyerAndSeller()
    {
        // Arrange
        var handler = new TransactionCreatedEventHandler(_unitOfWorkMock.Object);
        var @event = new TransactionCreatedEvent
        {
            TransactionId = 1,
            AuctionId = 1,
            Title = "Test Auction",
            BuyerId = 2,
            SellerId = 1,
            Amount = 100
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.BuyerId &&
                n.Type == NotificationType.TransactionCreated &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.Amount.ToString())
            )), Times.Once);

        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Type == NotificationType.TransactionCreated &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.Amount.ToString())
            )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionCompletedEventHandler_ShouldCreateNotifications_ForBuyerAndSeller()
    {
        // Arrange
        var handler = new TransactionCompletedEventHandler(_unitOfWorkMock.Object);
        var @event = new TransactionCompletedEvent
        {
            TransactionId = 1,
            AuctionId = 1,
            Title = "Test Auction",
            BuyerId = 2,
            SellerId = 1,
            Amount = 100
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.BuyerId &&
                n.Type == NotificationType.TransactionCompleted &&
                n.Title.Contains(@event.Title)
            )), Times.Once);

        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Type == NotificationType.TransactionCompleted &&
                n.Title.Contains(@event.Title)
            )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionFailedEventHandler_ShouldCreateNotifications_ForBuyerAndSeller()
    {
        // Arrange
        var handler = new TransactionFailedEventHandler(_unitOfWorkMock.Object);
        var @event = new TransactionFailedEvent
        {
            TransactionId = 1,
            AuctionId = 1,
            Title = "Test Auction",
            BuyerId = 2,
            SellerId = 1,
            Reason = "Payment failed"
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.BuyerId &&
                n.Type == NotificationType.TransactionFailed &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.Reason)
            )), Times.Once);

        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Type == NotificationType.TransactionFailed &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.Reason)
            )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionRefundedEventHandler_ShouldCreateNotifications_ForBuyerAndSeller()
    {
        // Arrange
        var handler = new TransactionRefundedEventHandler(_unitOfWorkMock.Object);
        var @event = new TransactionRefundedEvent
        {
            TransactionId = 1,
            AuctionId = 1,
            Title = "Test Auction",
            BuyerId = 2,
            SellerId = 1,
            Amount = 100,
            Reason = "Item not available"
        };

        // Act
        await handler.Handle(@event);

        // Assert
        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.BuyerId &&
                n.Type == NotificationType.TransactionRefunded &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.Amount.ToString()) &&
                n.Message.Contains(@event.Reason)
            )), Times.Once);

        _notificationRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Notification>(n =>
                n.UserId == @event.SellerId &&
                n.Type == NotificationType.TransactionRefunded &&
                n.Title.Contains(@event.Title) &&
                n.Message.Contains(@event.Amount.ToString()) &&
                n.Message.Contains(@event.Reason)
            )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}

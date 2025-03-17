using FluentAssertions;
using Moq;
using MzadPalestine.Application.Features.Notifications.EventHandlers;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events.Auctions;
using MzadPalestine.Core.Events.Bids;
using MzadPalestine.Core.Events.Transactions;
using MzadPalestine.Core.Interfaces;
using Xunit;

namespace MzadPalestine.Tests.Integration.Features.Notifications.EventHandlers;

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
    public async Task AuctionCreatedEventHandler_CreatesNotification_ForSeller()
    {
        // Arrange
        var auction = new Auction
        {
            Id = 1,
            SellerId = 1,
            Title = "Test Auction",
            ImageUrls = new List<string> { "test.jpg" }
        };

        var handler = new AuctionCreatedEventHandler(_unitOfWorkMock.Object);
        var @event = new AuctionCreatedEvent(auction);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _notificationRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Notification>(n =>
                n.UserId == auction.SellerId &&
                n.Title == "Auction Created Successfully" &&
                n.Type == NotificationType.AuctionCreated)),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task AuctionEndedEventHandler_CreatesNotifications_ForSellerAndWinner()
    {
        // Arrange
        var auction = new Auction
        {
            Id = 1,
            SellerId = 1,
            Title = "Test Auction",
            ImageUrls = new List<string> { "test.jpg" },
            WinningBidId = 1,
            WinningBid = new Bid { Id = 1, BidderId = 2, Amount = 100 }
        };

        var handler = new AuctionEndedEventHandler(_unitOfWorkMock.Object);
        var @event = new AuctionEndedEvent(auction);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _notificationRepositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 2 &&
                notifications.Any(n => n.UserId == auction.SellerId) &&
                notifications.Any(n => n.UserId == auction.WinningBid.BidderId))),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task BidPlacedEventHandler_CreatesNotifications_ForSellerAndPreviousBidder()
    {
        // Arrange
        var auction = new Auction
        {
            Id = 1,
            SellerId = 1,
            Title = "Test Auction",
            ImageUrls = new List<string> { "test.jpg" },
            Bids = new List<Bid>
            {
                new() { Id = 1, BidderId = 2, Amount = 90 }
            }
        };

        var newBid = new Bid
        {
            Id = 2,
            BidderId = 3,
            Amount = 100,
            Auction = auction
        };

        var handler = new BidPlacedEventHandler(_unitOfWorkMock.Object);
        var @event = new BidPlacedEvent(newBid);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _notificationRepositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 3 &&
                notifications.Any(n => n.UserId == auction.SellerId) &&
                notifications.Any(n => n.UserId == auction.Bids.First().BidderId) &&
                notifications.Any(n => n.UserId == newBid.BidderId))),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionCreatedEventHandler_CreatesNotifications_ForBuyerAndSeller()
    {
        // Arrange
        var auction = new Auction
        {
            Id = 1,
            Title = "Test Auction",
            ImageUrls = new List<string> { "test.jpg" }
        };

        var transaction = new Transaction
        {
            Id = 1,
            BuyerId = 2,
            SellerId = 1,
            Auction = auction
        };

        var handler = new TransactionCreatedEventHandler(_unitOfWorkMock.Object);
        var @event = new TransactionCreatedEvent(transaction);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _notificationRepositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 2 &&
                notifications.Any(n => n.UserId == transaction.SellerId) &&
                notifications.Any(n => n.UserId == transaction.BuyerId))),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionFailedEventHandler_CreatesNotifications_WithFailureReason()
    {
        // Arrange
        var auction = new Auction
        {
            Id = 1,
            Title = "Test Auction",
            ImageUrls = new List<string> { "test.jpg" }
        };

        var transaction = new Transaction
        {
            Id = 1,
            BuyerId = 2,
            SellerId = 1,
            Auction = auction
        };

        const string failureReason = "Payment declined";

        var handler = new TransactionFailedEventHandler(_unitOfWorkMock.Object);
        var @event = new TransactionFailedEvent(transaction, failureReason);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _notificationRepositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 2 &&
                notifications.All(n => n.Message.Contains(failureReason)))),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionRefundedEventHandler_CreatesNotifications_WithRefundReason()
    {
        // Arrange
        var auction = new Auction
        {
            Id = 1,
            Title = "Test Auction",
            ImageUrls = new List<string> { "test.jpg" }
        };

        var transaction = new Transaction
        {
            Id = 1,
            BuyerId = 2,
            SellerId = 1,
            Auction = auction
        };

        const string refundReason = "Item not as described";

        var handler = new TransactionRefundedEventHandler(_unitOfWorkMock.Object);
        var @event = new TransactionRefundedEvent(transaction, refundReason);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _notificationRepositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
                notifications.Count() == 2 &&
                notifications.All(n => n.Message.Contains(refundReason)))),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.CompleteAsync(), Times.Once);
    }
}

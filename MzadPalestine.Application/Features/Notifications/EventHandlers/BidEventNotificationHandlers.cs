using MediatR;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events.Bids;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.EventHandlers;

public class BidPlacedEventHandler : INotificationHandler<BidPlacedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public BidPlacedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BidPlacedEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Bid.Auction.SellerId,
            Title = "New Bid Received",
            Message = $"A new bid of {notification.Bid.Amount:C} has been placed on your auction '{notification.Bid.Auction.Title}'",
            Type = NotificationType.BidPlaced,
            ActionUrl = $"/auctions/{notification.Bid.AuctionId}/bids",
            ImageUrl = notification.Bid.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        // Notify previous highest bidder if they were outbid
        var previousHighestBid = notification.Bid.Auction.Bids
            .Where(b => b.Id != notification.Bid.Id)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefault();

        if (previousHighestBid != null && previousHighestBid.Amount < notification.Bid.Amount)
        {
            notifications.Add(new Notification
            {
                UserId = previousHighestBid.BidderId,
                Title = "You've Been Outbid",
                Message = $"Someone has placed a higher bid of {notification.Bid.Amount:C} on '{notification.Bid.Auction.Title}'",
                Type = NotificationType.OutBid,
                ActionUrl = $"/auctions/{notification.Bid.AuctionId}",
                ImageUrl = notification.Bid.Auction.ImageUrls?.FirstOrDefault(),
                CreatedAt = DateTime.UtcNow
            });
        }

        // Notify bidder of successful bid
        notifications.Add(new Notification
        {
            UserId = notification.Bid.BidderId,
            Title = "Bid Placed Successfully",
            Message = $"Your bid of {notification.Bid.Amount:C} on '{notification.Bid.Auction.Title}' has been placed successfully",
            Type = NotificationType.BidPlaced,
            ActionUrl = $"/auctions/{notification.Bid.AuctionId}",
            ImageUrl = notification.Bid.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

public class BidCancelledEventHandler : INotificationHandler<BidCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public BidCancelledEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BidCancelledEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Bid.Auction.SellerId,
            Title = "Bid Cancelled",
            Message = $"A bid of {notification.Bid.Amount:C} has been cancelled on your auction '{notification.Bid.Auction.Title}'",
            Type = NotificationType.BidCancelled,
            ActionUrl = $"/auctions/{notification.Bid.AuctionId}/bids",
            ImageUrl = notification.Bid.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        // Notify bidder
        notifications.Add(new Notification
        {
            UserId = notification.Bid.BidderId,
            Title = "Bid Cancelled",
            Message = $"Your bid of {notification.Bid.Amount:C} on '{notification.Bid.Auction.Title}' has been cancelled",
            Type = NotificationType.BidCancelled,
            ActionUrl = $"/auctions/{notification.Bid.AuctionId}",
            ImageUrl = notification.Bid.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

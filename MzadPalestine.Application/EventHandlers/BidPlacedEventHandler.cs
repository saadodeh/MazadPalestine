using MediatR;
using Microsoft.Extensions.Logging;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.EventHandlers;

public class BidPlacedEventHandler : INotificationHandler<BidPlacedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BidPlacedEventHandler> _logger;

    public BidPlacedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<BidPlacedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BidPlacedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(notification.AuctionId);
            if (auction == null)
            {
                _logger.LogWarning(
                    "Auction {AuctionId} not found while handling BidPlacedEvent",
                    notification.AuctionId);
                return;
            }

            // Create notification for the seller
            var sellerNotification = new Notification
            {
                UserId = auction.SellerId,
                Title = "New Bid Received",
                Message = $"A new bid of {notification.BidAmount} has been placed on your auction '{auction.Title}'",
                Type = NotificationType.BidReceived,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Notification>().Add(sellerNotification);

            // If this is a new highest bid, notify the previous highest bidder
            if (notification.IsHighestBid && notification.PreviousHighestBidderId.HasValue)
            {
                var outbidNotification = new Notification
                {
                    UserId = notification.PreviousHighestBidderId.Value,
                    Title = "You've Been Outbid",
                    Message = $"Someone has placed a higher bid of {notification.BidAmount} on '{auction.Title}'",
                    Type = NotificationType.Outbid,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Repository<Notification>().Add(outbidNotification);
            }

            // Create notification for the bidder
            var bidderNotification = new Notification
            {
                UserId = notification.BidderId,
                Title = notification.IsHighestBid ? "Highest Bid Placed!" : "Bid Placed Successfully",
                Message = notification.IsHighestBid
                    ? $"Your bid of {notification.BidAmount} is currently the highest bid on '{auction.Title}'"
                    : $"Your bid of {notification.BidAmount} has been placed on '{auction.Title}', but it's not the highest bid",
                Type = NotificationType.BidPlaced,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Notification>().Add(bidderNotification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Created notifications for bid placement on auction {AuctionId}",
                notification.AuctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling BidPlacedEvent for auction {AuctionId}",
                notification.AuctionId);
        }
    }
}

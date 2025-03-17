using MediatR;
using Microsoft.Extensions.Logging;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.EventHandlers;

public class BidCancelledEventHandler : INotificationHandler<BidCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BidCancelledEventHandler> _logger;

    public BidCancelledEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<BidCancelledEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BidCancelledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(notification.AuctionId);
            if (auction == null)
            {
                _logger.LogWarning(
                    "Auction {AuctionId} not found while handling BidCancelledEvent",
                    notification.AuctionId);
                return;
            }

            // Create notification for the bidder
            var bidderNotification = new Notification
            {
                UserId = notification.BidderId,
                Title = "Bid Cancelled",
                Message = $"Your bid of {notification.BidAmount} on '{auction.Title}' has been cancelled. " +
                         "Any pending amount will be refunded.",
                Type = NotificationType.BidCancelled,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Notification>().Add(bidderNotification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Created notification for bid {BidId} cancellation on auction {AuctionId}",
                notification.BidId,
                notification.AuctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling BidCancelledEvent for bid {BidId} on auction {AuctionId}",
                notification.BidId,
                notification.AuctionId);
        }
    }
}

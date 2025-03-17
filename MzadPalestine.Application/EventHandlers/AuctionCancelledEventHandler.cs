using MediatR;
using Microsoft.Extensions.Logging;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.EventHandlers;

public class AuctionCancelledEventHandler : INotificationHandler<AuctionCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionCancelledEventHandler> _logger;

    public AuctionCancelledEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<AuctionCancelledEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AuctionCancelledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get all bidders for this auction
            var bids = await _unitOfWork.Repository<Bid>()
                .ListAsync(x => x.AuctionId == notification.AuctionId);

            var bidderIds = bids.Select(b => b.UserId).Distinct().ToList();

            // Create notifications for all bidders
            foreach (var bidderId in bidderIds)
            {
                var bidderNotification = new Notification
                {
                    UserId = bidderId,
                    Title = "Auction Cancelled",
                    Message = $"The auction '{notification.Title}' has been cancelled by the seller. " +
                             "Any pending bids will be refunded.",
                    Type = NotificationType.AuctionCancelled,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Repository<Notification>().Add(bidderNotification);
            }

            // Create notification for seller
            var sellerNotification = new Notification
            {
                UserId = notification.SellerId,
                Title = "Auction Cancelled Successfully",
                Message = $"Your auction '{notification.Title}' has been cancelled successfully. " +
                         "All bidders have been notified.",
                Type = NotificationType.AuctionCancelled,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Notification>().Add(sellerNotification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Created notifications for auction {AuctionId} cancellation",
                notification.AuctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling AuctionCancelledEvent for auction {AuctionId}",
                notification.AuctionId);
        }
    }
}

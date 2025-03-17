using MediatR;
using Microsoft.Extensions.Logging;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.EventHandlers;

public class AuctionEndedEventHandler : INotificationHandler<AuctionEndedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionEndedEventHandler> _logger;

    public AuctionEndedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<AuctionEndedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AuctionEndedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(notification.AuctionId);
            if (auction == null)
            {
                _logger.LogWarning(
                    "Auction {AuctionId} not found while handling AuctionEndedEvent",
                    notification.AuctionId);
                return;
            }

            // Create notification for seller
            var sellerNotification = new Notification
            {
                UserId = notification.SellerId,
                Title = "Auction Ended",
                Message = notification.HasWinner
                    ? $"Your auction '{auction.Title}' has ended with a winning bid of {notification.FinalPrice}"
                    : $"Your auction '{auction.Title}' has ended without any bids",
                Type = NotificationType.AuctionEnded,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Notification>().Add(sellerNotification);

            // If there's a winner, create notification for them
            if (notification.WinnerId.HasValue)
            {
                var winnerNotification = new Notification
                {
                    UserId = notification.WinnerId.Value,
                    Title = "Auction Won!",
                    Message = $"Congratulations! You won the auction '{auction.Title}' with your bid of {notification.FinalPrice}",
                    Type = NotificationType.AuctionWon,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Repository<Notification>().Add(winnerNotification);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Created notifications for auction {AuctionId} completion",
                notification.AuctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling AuctionEndedEvent for auction {AuctionId}",
                notification.AuctionId);
        }
    }
}

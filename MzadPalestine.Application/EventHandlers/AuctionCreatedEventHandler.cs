using MediatR;
using Microsoft.Extensions.Logging;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.EventHandlers;

public class AuctionCreatedEventHandler : INotificationHandler<AuctionCreatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionCreatedEventHandler> _logger;

    public AuctionCreatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<AuctionCreatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AuctionCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Create notification for seller
            var notification = new Notification
            {
                UserId = notification.SellerId,
                Title = "Auction Created Successfully",
                Message = $"Your auction '{notification.Title}' has been created successfully with a starting price of {notification.StartingPrice}. " +
                         $"The auction will end on {notification.EndTime:g}",
                Type = NotificationType.AuctionCreated,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Notification>().Add(notification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Created notification for auction {AuctionId} creation",
                notification.AuctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling AuctionCreatedEvent for auction {AuctionId}",
                notification.AuctionId);
        }
    }
}

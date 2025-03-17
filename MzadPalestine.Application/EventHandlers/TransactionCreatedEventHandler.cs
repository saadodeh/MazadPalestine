using MediatR;
using Microsoft.Extensions.Logging;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.EventHandlers;

public class TransactionCreatedEventHandler : INotificationHandler<TransactionCreatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionCreatedEventHandler> _logger;

    public TransactionCreatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<TransactionCreatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TransactionCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(notification.AuctionId);
            if (auction == null)
            {
                _logger.LogWarning(
                    "Auction {AuctionId} not found while handling TransactionCreatedEvent",
                    notification.AuctionId);
                return;
            }

            // Create notification for the buyer
            var buyerNotification = new Notification
            {
                UserId = notification.BuyerId,
                Title = "Payment Required",
                Message = $"Please complete the payment of {notification.Amount} {notification.Currency} " +
                         $"for winning the auction '{auction.Title}'. The seller will be notified once the payment is processed.",
                Type = NotificationType.PaymentRequired,
                CreatedAt = DateTime.UtcNow
            };

            // Create notification for the seller
            var sellerNotification = new Notification
            {
                UserId = notification.SellerId,
                Title = "Transaction Created",
                Message = $"A transaction for {notification.Amount} {notification.Currency} has been created " +
                         $"for your auction '{auction.Title}'. You will be notified once the buyer completes the payment.",
                Type = NotificationType.TransactionCreated,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Notification>().Add(buyerNotification);
            _unitOfWork.Repository<Notification>().Add(sellerNotification);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Created notifications for transaction {TransactionId} creation",
                notification.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling TransactionCreatedEvent for transaction {TransactionId}",
                notification.TransactionId);
        }
    }
}

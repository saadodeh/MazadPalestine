using MediatR;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events.Transactions;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.EventHandlers;

public class TransactionCreatedEventHandler : INotificationHandler<TransactionCreatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionCreatedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionCreatedEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Transaction.SellerId,
            Title = "Payment Required",
            Message = $"A transaction has been created for your auction '{notification.Transaction.Auction.Title}'. Please complete the payment process.",
            Type = NotificationType.TransactionCreated,
            ActionUrl = $"/transactions/{notification.Transaction.Id}/payment",
            ImageUrl = notification.Transaction.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        // Notify buyer
        notifications.Add(new Notification
        {
            UserId = notification.Transaction.BuyerId,
            Title = "Payment Required",
            Message = $"A transaction has been created for the auction '{notification.Transaction.Auction.Title}'. Please complete the payment process.",
            Type = NotificationType.TransactionCreated,
            ActionUrl = $"/transactions/{notification.Transaction.Id}/payment",
            ImageUrl = notification.Transaction.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

public class TransactionCompletedEventHandler : INotificationHandler<TransactionCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionCompletedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionCompletedEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Transaction.SellerId,
            Title = "Payment Completed",
            Message = $"The payment for your auction '{notification.Transaction.Auction.Title}' has been completed successfully.",
            Type = NotificationType.TransactionCompleted,
            ActionUrl = $"/transactions/{notification.Transaction.Id}",
            ImageUrl = notification.Transaction.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        // Notify buyer
        notifications.Add(new Notification
        {
            UserId = notification.Transaction.BuyerId,
            Title = "Payment Completed",
            Message = $"Your payment for the auction '{notification.Transaction.Auction.Title}' has been completed successfully.",
            Type = NotificationType.TransactionCompleted,
            ActionUrl = $"/transactions/{notification.Transaction.Id}",
            ImageUrl = notification.Transaction.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

public class TransactionFailedEventHandler : INotificationHandler<TransactionFailedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionFailedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionFailedEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Transaction.SellerId,
            Title = "Payment Failed",
            Message = $"The payment for your auction '{notification.Transaction.Auction.Title}' has failed. Reason: {notification.FailureReason}",
            Type = NotificationType.TransactionFailed,
            ActionUrl = $"/transactions/{notification.Transaction.Id}",
            ImageUrl = notification.Transaction.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        // Notify buyer
        notifications.Add(new Notification
        {
            UserId = notification.Transaction.BuyerId,
            Title = "Payment Failed",
            Message = $"Your payment for the auction '{notification.Transaction.Auction.Title}' has failed. Reason: {notification.FailureReason}",
            Type = NotificationType.TransactionFailed,
            ActionUrl = $"/transactions/{notification.Transaction.Id}/payment",
            ImageUrl = notification.Transaction.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

public class TransactionRefundedEventHandler : INotificationHandler<TransactionRefundedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionRefundedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionRefundedEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Transaction.SellerId,
            Title = "Payment Refunded",
            Message = $"The payment for your auction '{notification.Transaction.Auction.Title}' has been refunded. Reason: {notification.RefundReason}",
            Type = NotificationType.TransactionRefunded,
            ActionUrl = $"/transactions/{notification.Transaction.Id}",
            ImageUrl = notification.Transaction.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        // Notify buyer
        notifications.Add(new Notification
        {
            UserId = notification.Transaction.BuyerId,
            Title = "Payment Refunded",
            Message = $"Your payment for the auction '{notification.Transaction.Auction.Title}' has been refunded. Reason: {notification.RefundReason}",
            Type = NotificationType.TransactionRefunded,
            ActionUrl = $"/transactions/{notification.Transaction.Id}",
            ImageUrl = notification.Transaction.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

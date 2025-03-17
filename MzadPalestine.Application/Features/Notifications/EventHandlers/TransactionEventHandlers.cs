using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.EventHandlers;

public class TransactionCreatedEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionCreatedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionCreatedEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify buyer about required payment
        notifications.Add(new Notification
        {
            UserId = @event.BuyerId,
            Title = "Payment Required",
            Message = $"Please complete your payment of ${@event.Amount} for '{@event.Title}'. Due by: {@event.DueDate:g}",
            Type = NotificationType.PaymentRequired,
            ActionUrl = $"/transactions/{@event.TransactionId}/pay",
            CreatedAt = DateTime.UtcNow
        });

        // Notify seller about pending payment
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "Payment Pending",
            Message = $"A payment of ${@event.Amount} for '{@event.Title}' is pending from the buyer",
            Type = NotificationType.TransactionCreated,
            ActionUrl = $"/transactions/{@event.TransactionId}",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class TransactionCompletedEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionCompletedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionCompletedEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify buyer about successful payment
        notifications.Add(new Notification
        {
            UserId = @event.BuyerId,
            Title = "Payment Successful",
            Message = $"Your payment of ${@event.Amount} for '{@event.Title}' has been completed successfully",
            Type = NotificationType.PaymentSent,
            ActionUrl = $"/transactions/{@event.TransactionId}",
            CreatedAt = DateTime.UtcNow
        });

        // Notify seller about received payment
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "Payment Received",
            Message = $"You have received a payment of ${@event.Amount} for '{@event.Title}'",
            Type = NotificationType.PaymentReceived,
            ActionUrl = $"/transactions/{@event.TransactionId}",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class TransactionFailedEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionFailedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionFailedEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify buyer about failed payment
        notifications.Add(new Notification
        {
            UserId = @event.BuyerId,
            Title = "Payment Failed",
            Message = $"Your payment for '{@event.Title}' has failed. Reason: {@event.Reason}",
            Type = NotificationType.TransactionFailed,
            ActionUrl = $"/transactions/{@event.TransactionId}/retry",
            CreatedAt = DateTime.UtcNow
        });

        // Notify seller about failed payment
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "Payment Failed",
            Message = $"The buyer's payment for '{@event.Title}' has failed. Reason: {@event.Reason}",
            Type = NotificationType.TransactionFailed,
            ActionUrl = $"/transactions/{@event.TransactionId}",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class TransactionRefundedEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionRefundedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransactionRefundedEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify buyer about refund
        notifications.Add(new Notification
        {
            UserId = @event.BuyerId,
            Title = "Payment Refunded",
            Message = $"Your payment of ${@event.Amount} for '{@event.Title}' has been refunded. Reason: {@event.Reason}",
            Type = NotificationType.TransactionRefunded,
            ActionUrl = $"/transactions/{@event.TransactionId}",
            CreatedAt = DateTime.UtcNow
        });

        // Notify seller about refund
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "Payment Refunded",
            Message = $"The payment of ${@event.Amount} for '{@event.Title}' has been refunded to the buyer. Reason: {@event.Reason}",
            Type = NotificationType.TransactionRefunded,
            ActionUrl = $"/transactions/{@event.TransactionId}",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

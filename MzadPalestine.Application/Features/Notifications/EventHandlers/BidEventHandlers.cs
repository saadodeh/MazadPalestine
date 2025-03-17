using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.EventHandlers;

public class BidPlacedEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public BidPlacedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BidPlacedEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify seller about new bid
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "New Bid Received",
            Message = $"A new bid of ${@event.Amount} has been placed on your auction '{@event.Title}'",
            Type = NotificationType.BidReceived,
            ActionUrl = $"/auctions/{@event.AuctionId}",
            CreatedAt = DateTime.UtcNow
        });

        // Notify previous highest bidder that they've been outbid
        if (@event.PreviousBidderId.HasValue)
        {
            notifications.Add(new Notification
            {
                UserId = @event.PreviousBidderId.Value,
                Title = "You've Been Outbid!",
                Message = $"Someone has placed a higher bid of ${@event.Amount} on '{@event.Title}'",
                Type = NotificationType.Outbid,
                ActionUrl = $"/auctions/{@event.AuctionId}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class BidCancelledEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public BidCancelledEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BidCancelledEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify seller about cancelled bid
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "Bid Cancelled",
            Message = string.IsNullOrEmpty(@event.Reason)
                ? $"A bid of ${@event.Amount} on your auction '{@event.Title}' has been cancelled"
                : $"A bid of ${@event.Amount} on your auction '{@event.Title}' has been cancelled. Reason: {@event.Reason}",
            Type = NotificationType.BidCancelled,
            ActionUrl = $"/auctions/{@event.AuctionId}",
            CreatedAt = DateTime.UtcNow
        });

        // Notify bidder about their cancelled bid
        notifications.Add(new Notification
        {
            UserId = @event.BidderId,
            Title = "Bid Cancelled",
            Message = string.IsNullOrEmpty(@event.Reason)
                ? $"Your bid of ${@event.Amount} on '{@event.Title}' has been cancelled"
                : $"Your bid of ${@event.Amount} on '{@event.Title}' has been cancelled. Reason: {@event.Reason}",
            Type = NotificationType.BidCancelled,
            ActionUrl = $"/auctions/{@event.AuctionId}",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class OutbidEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public OutbidEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OutbidEvent @event)
    {
        var notification = new Notification
        {
            UserId = @event.BidderId,
            Title = "You've Been Outbid!",
            Message = $"Your bid of ${@event.PreviousAmount} on '{@event.Title}' has been outbid. The new highest bid is ${@event.NewAmount}",
            Type = NotificationType.Outbid,
            ActionUrl = $"/auctions/{@event.AuctionId}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }
}

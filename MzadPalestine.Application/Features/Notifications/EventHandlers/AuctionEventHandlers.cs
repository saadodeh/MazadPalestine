using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.EventHandlers;

public class AuctionCreatedEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public AuctionCreatedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AuctionCreatedEvent @event)
    {
        var notification = new Notification
        {
            UserId = @event.SellerId,
            Title = "Auction Created Successfully",
            Message = $"Your auction '{@event.Title}' has been created with a starting price of ${@event.StartingPrice}",
            Type = NotificationType.AuctionCreated,
            ActionUrl = $"/auctions/{@event.AuctionId}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class AuctionEndedEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public AuctionEndedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AuctionEndedEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "Auction Ended",
            Message = @event.WinnerId.HasValue
                ? $"Your auction '{@event.Title}' has ended with a winning bid of ${@event.FinalPrice}"
                : $"Your auction '{@event.Title}' has ended without any bids",
            Type = NotificationType.AuctionEnded,
            ActionUrl = $"/auctions/{@event.AuctionId}",
            CreatedAt = DateTime.UtcNow
        });

        // Notify winner if exists
        if (@event.WinnerId.HasValue)
        {
            notifications.Add(new Notification
            {
                UserId = @event.WinnerId.Value,
                Title = "Auction Won!",
                Message = $"Congratulations! You won the auction '{@event.Title}' with a bid of ${@event.FinalPrice}",
                Type = NotificationType.AuctionWon,
                ActionUrl = $"/auctions/{@event.AuctionId}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class AuctionCancelledEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public AuctionCancelledEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AuctionCancelledEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "Auction Cancelled",
            Message = string.IsNullOrEmpty(@event.Reason)
                ? $"Your auction '{@event.Title}' has been cancelled"
                : $"Your auction '{@event.Title}' has been cancelled. Reason: {@event.Reason}",
            Type = NotificationType.AuctionCancelled,
            ActionUrl = $"/auctions/{@event.AuctionId}",
            CreatedAt = DateTime.UtcNow
        });

        // Notify all bidders
        foreach (var bidderId in @event.BidderIds)
        {
            notifications.Add(new Notification
            {
                UserId = bidderId,
                Title = "Auction Cancelled",
                Message = string.IsNullOrEmpty(@event.Reason)
                    ? $"The auction '{@event.Title}' has been cancelled"
                    : $"The auction '{@event.Title}' has been cancelled. Reason: {@event.Reason}",
                Type = NotificationType.AuctionCancelled,
                ActionUrl = $"/auctions/{@event.AuctionId}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class AuctionDeletedEventHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public AuctionDeletedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AuctionDeletedEvent @event)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = @event.SellerId,
            Title = "Auction Deleted",
            Message = $"Your auction '{@event.Title}' has been deleted",
            Type = NotificationType.AuctionDeleted,
            CreatedAt = DateTime.UtcNow
        });

        // Notify all bidders
        foreach (var bidderId in @event.BidderIds)
        {
            notifications.Add(new Notification
            {
                UserId = bidderId,
                Title = "Auction Deleted",
                Message = $"The auction '{@event.Title}' has been deleted",
                Type = NotificationType.AuctionDeleted,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();
    }
}

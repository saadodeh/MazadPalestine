using MediatR;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events.Auctions;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Notifications.EventHandlers;

public class AuctionCreatedEventHandler : INotificationHandler<AuctionCreatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public AuctionCreatedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AuctionCreatedEvent notification, CancellationToken cancellationToken)
    {
        var newNotification = new Notification
        {
            UserId = notification.Auction.SellerId,
            Title = "Auction Created Successfully",
            Message = $"Your auction '{notification.Auction.Title}' has been created successfully.",
            Type = NotificationType.AuctionCreated,
            ActionUrl = $"/auctions/{notification.Auction.Id}",
            ImageUrl = notification.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(newNotification);
        await _unitOfWork.CompleteAsync();
    }
}

public class AuctionEndedEventHandler : INotificationHandler<AuctionEndedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public AuctionEndedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AuctionEndedEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Auction.SellerId,
            Title = "Auction Ended",
            Message = notification.Auction.WinningBidId.HasValue
                ? $"Your auction '{notification.Auction.Title}' has ended with a winning bid of {notification.Auction.WinningBid?.Amount:C}"
                : $"Your auction '{notification.Auction.Title}' has ended without any bids.",
            Type = NotificationType.AuctionEnded,
            ActionUrl = $"/auctions/{notification.Auction.Id}",
            ImageUrl = notification.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        // Notify winner if exists
        if (notification.Auction.WinningBidId.HasValue && notification.Auction.WinningBid != null)
        {
            notifications.Add(new Notification
            {
                UserId = notification.Auction.WinningBid.BidderId,
                Title = "You Won the Auction!",
                Message = $"Congratulations! You won the auction '{notification.Auction.Title}' with your bid of {notification.Auction.WinningBid.Amount:C}",
                Type = NotificationType.AuctionWon,
                ActionUrl = $"/auctions/{notification.Auction.Id}/payment",
                ImageUrl = notification.Auction.ImageUrls?.FirstOrDefault(),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

public class AuctionCancelledEventHandler : INotificationHandler<AuctionCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public AuctionCancelledEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AuctionCancelledEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Auction.SellerId,
            Title = "Auction Cancelled",
            Message = $"Your auction '{notification.Auction.Title}' has been cancelled.",
            Type = NotificationType.AuctionCancelled,
            ActionUrl = $"/auctions/{notification.Auction.Id}",
            ImageUrl = notification.Auction.ImageUrls?.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });

        // Notify all bidders
        var bidderIds = notification.Auction.Bids
            .Select(b => b.BidderId)
            .Distinct();

        foreach (var bidderId in bidderIds)
        {
            notifications.Add(new Notification
            {
                UserId = bidderId,
                Title = "Auction Cancelled",
                Message = $"The auction '{notification.Auction.Title}' has been cancelled by the seller.",
                Type = NotificationType.AuctionCancelled,
                ActionUrl = $"/auctions/{notification.Auction.Id}",
                ImageUrl = notification.Auction.ImageUrls?.FirstOrDefault(),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

public class AuctionDeletedEventHandler : INotificationHandler<AuctionDeletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public AuctionDeletedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AuctionDeletedEvent notification, CancellationToken cancellationToken)
    {
        var notifications = new List<Notification>();

        // Notify seller
        notifications.Add(new Notification
        {
            UserId = notification.Auction.SellerId,
            Title = "Auction Deleted",
            Message = $"Your auction '{notification.Auction.Title}' has been deleted.",
            Type = NotificationType.AuctionDeleted,
            CreatedAt = DateTime.UtcNow
        });

        // Notify all bidders
        var bidderIds = notification.Auction.Bids
            .Select(b => b.BidderId)
            .Distinct();

        foreach (var bidderId in bidderIds)
        {
            notifications.Add(new Notification
            {
                UserId = bidderId,
                Title = "Auction Deleted",
                Message = $"An auction you bid on '{notification.Auction.Title}' has been deleted.",
                Type = NotificationType.AuctionDeleted,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CompleteAsync();
    }
}

using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Events;

public class AuctionUpdatedEvent : BaseDomainEvent
{
    public int AuctionId { get; }
    public int SellerId { get; }
    public string Title { get; }
    public AuctionStatus Status { get; }

    public AuctionUpdatedEvent(int auctionId, int sellerId, string title, AuctionStatus status)
    {
        AuctionId = auctionId;
        SellerId = sellerId;
        Title = title;
        Status = status;
    }
}

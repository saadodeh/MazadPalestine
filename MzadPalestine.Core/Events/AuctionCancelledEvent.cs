namespace MzadPalestine.Core.Events;

public class AuctionCancelledEvent : BaseDomainEvent
{
    public int AuctionId { get; }
    public int SellerId { get; }
    public string Title { get; }

    public AuctionCancelledEvent(int auctionId, int sellerId, string title)
    {
        AuctionId = auctionId;
        SellerId = sellerId;
        Title = title;
    }
}

namespace MzadPalestine.Core.Events;

public class AuctionDeletedEvent : BaseDomainEvent
{
    public int AuctionId { get; }
    public int SellerId { get; }
    public string Title { get; }

    public AuctionDeletedEvent(int auctionId, int sellerId, string title)
    {
        AuctionId = auctionId;
        SellerId = sellerId;
        Title = title;
    }
}

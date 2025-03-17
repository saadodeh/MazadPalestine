namespace MzadPalestine.Core.Events;

public class BidCancelledEvent : BaseDomainEvent
{
    public int BidId { get; }
    public int AuctionId { get; }
    public int BidderId { get; }
    public decimal BidAmount { get; }

    public BidCancelledEvent(int bidId, int auctionId, int bidderId, decimal bidAmount)
    {
        BidId = bidId;
        AuctionId = auctionId;
        BidderId = bidderId;
        BidAmount = bidAmount;
    }
}

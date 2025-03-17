namespace MzadPalestine.Core.Events;

public class BidPlacedEvent : BaseDomainEvent
{
    public int AuctionId { get; }
    public int BidderId { get; }
    public decimal BidAmount { get; }
    public bool IsHighestBid { get; }
    public int? PreviousHighestBidderId { get; }

    public BidPlacedEvent(int auctionId, int bidderId, decimal bidAmount, bool isHighestBid, int? previousHighestBidderId = null)
    {
        AuctionId = auctionId;
        BidderId = bidderId;
        BidAmount = bidAmount;
        IsHighestBid = isHighestBid;
        PreviousHighestBidderId = previousHighestBidderId;
    }
}

namespace MzadPalestine.Core.Events;

public class AuctionEndedEvent : BaseDomainEvent
{
    public int AuctionId { get; }
    public int SellerId { get; }
    public int? WinnerId { get; }
    public decimal? FinalPrice { get; }
    public bool HasWinner { get; }

    public AuctionEndedEvent(int auctionId, int sellerId, int? winnerId, decimal? finalPrice)
    {
        AuctionId = auctionId;
        SellerId = sellerId;
        WinnerId = winnerId;
        FinalPrice = finalPrice;
        HasWinner = winnerId.HasValue;
    }
}

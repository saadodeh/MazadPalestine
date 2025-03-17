namespace MzadPalestine.Core.Events;

public class AuctionCreatedEvent : BaseDomainEvent
{
    public int AuctionId { get; }
    public int SellerId { get; }
    public string Title { get; }
    public decimal StartingPrice { get; }
    public DateTime EndTime { get; }

    public AuctionCreatedEvent(int auctionId, int sellerId, string title, decimal startingPrice, DateTime endTime)
    {
        AuctionId = auctionId;
        SellerId = sellerId;
        Title = title;
        StartingPrice = startingPrice;
        EndTime = endTime;
    }
}

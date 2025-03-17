namespace MzadPalestine.Core.Events;

public record AuctionCreatedEvent
{
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int SellerId { get; init; }
    public decimal StartingPrice { get; init; }
}

public record AuctionEndedEvent
{
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int SellerId { get; init; }
    public int? WinnerId { get; init; }
    public decimal FinalPrice { get; init; }
}

public record AuctionCancelledEvent
{
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int SellerId { get; init; }
    public List<int> BidderIds { get; init; } = new();
    public string? Reason { get; init; }
}

public record AuctionDeletedEvent
{
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int SellerId { get; init; }
    public List<int> BidderIds { get; init; } = new();
}

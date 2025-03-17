namespace MzadPalestine.Core.Events;

public record BidPlacedEvent
{
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int BidderId { get; init; }
    public int SellerId { get; init; }
    public int? PreviousBidderId { get; init; }
    public decimal Amount { get; init; }
}

public record BidCancelledEvent
{
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int BidderId { get; init; }
    public int SellerId { get; init; }
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
}

public record OutbidEvent
{
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int BidderId { get; init; }
    public int NewBidderId { get; init; }
    public decimal NewAmount { get; init; }
    public decimal PreviousAmount { get; init; }
}

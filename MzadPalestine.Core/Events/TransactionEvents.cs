namespace MzadPalestine.Core.Events;

public record TransactionCreatedEvent
{
    public int TransactionId { get; init; }
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int BuyerId { get; init; }
    public int SellerId { get; init; }
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
}

public record TransactionCompletedEvent
{
    public int TransactionId { get; init; }
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int BuyerId { get; init; }
    public int SellerId { get; init; }
    public decimal Amount { get; init; }
    public DateTime CompletedAt { get; init; }
}

public record TransactionFailedEvent
{
    public int TransactionId { get; init; }
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int BuyerId { get; init; }
    public int SellerId { get; init; }
    public string Reason { get; init; } = null!;
    public DateTime FailedAt { get; init; }
}

public record TransactionRefundedEvent
{
    public int TransactionId { get; init; }
    public int AuctionId { get; init; }
    public string Title { get; init; } = null!;
    public int BuyerId { get; init; }
    public int SellerId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = null!;
    public DateTime RefundedAt { get; init; }
}

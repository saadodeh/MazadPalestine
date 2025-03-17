using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Events;

public class TransactionCreatedEvent : BaseDomainEvent
{
    public int TransactionId { get; }
    public int AuctionId { get; }
    public int BuyerId { get; }
    public int SellerId { get; }
    public decimal Amount { get; }
    public Currency Currency { get; }

    public TransactionCreatedEvent(
        int transactionId,
        int auctionId,
        int buyerId,
        int sellerId,
        decimal amount,
        Currency currency)
    {
        TransactionId = transactionId;
        AuctionId = auctionId;
        BuyerId = buyerId;
        SellerId = sellerId;
        Amount = amount;
        Currency = currency;
    }
}

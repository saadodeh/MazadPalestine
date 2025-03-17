using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Events;

public class PaymentProcessedEvent : BaseDomainEvent
{
    public int TransactionId { get; }
    public int UserId { get; }
    public int? AuctionId { get; }
    public decimal Amount { get; }
    public Currency Currency { get; }
    public PaymentMethod PaymentMethod { get; }
    public string TransactionReference { get; }
    public bool IsSuccessful { get; }

    public PaymentProcessedEvent(
        int transactionId,
        int userId,
        decimal amount,
        Currency currency,
        PaymentMethod paymentMethod,
        string transactionReference,
        bool isSuccessful,
        int? auctionId = null)
    {
        TransactionId = transactionId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
        PaymentMethod = paymentMethod;
        TransactionReference = transactionReference;
        IsSuccessful = isSuccessful;
        AuctionId = auctionId;
    }
}

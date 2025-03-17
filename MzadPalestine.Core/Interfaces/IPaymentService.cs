using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Interfaces;

public interface IPaymentService
{
    Task<Transaction> ProcessPaymentAsync(
        decimal amount,
        Currency currency,
        PaymentMethod paymentMethod,
        int userId,
        int? auctionId = null);

    Task<Transaction> ProcessRefundAsync(
        Transaction originalTransaction,
        string reason);

    Task<bool> ValidatePaymentAsync(string transactionReference);
    
    Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId);
    
    Task<Transaction?> GetTransactionByReferenceAsync(string transactionReference);
}

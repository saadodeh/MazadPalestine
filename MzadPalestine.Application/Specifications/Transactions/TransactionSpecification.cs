using MzadPalestine.Application.Common.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.Specifications.Transactions;

public class TransactionSpecification : BaseSpecification<Transaction>
{
    public TransactionSpecification(int? userId = null, int? auctionId = null, 
        TransactionStatus? status = null, PaymentMethod? paymentMethod = null,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        // Include navigation properties
        AddInclude(t => t.User);
        AddInclude(t => t.Auction);

        // Apply filters
        if (userId.HasValue)
        {
            And(t => t.UserId == userId);
        }

        if (auctionId.HasValue)
        {
            And(t => t.AuctionId == auctionId);
        }

        if (status.HasValue)
        {
            And(t => t.Status == status);
        }

        if (paymentMethod.HasValue)
        {
            And(t => t.PaymentMethod == paymentMethod);
        }

        if (startDate.HasValue)
        {
            And(t => t.CreatedAt >= startDate);
        }

        if (endDate.HasValue)
        {
            And(t => t.CreatedAt <= endDate);
        }

        // Default ordering by most recent
        ApplyOrderByDescending(t => t.CreatedAt);
    }
}

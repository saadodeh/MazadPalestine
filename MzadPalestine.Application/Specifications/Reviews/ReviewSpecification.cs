using MzadPalestine.Application.Common.Specifications;
using MzadPalestine.Core.Entities;

namespace MzadPalestine.Application.Specifications.Reviews;

public class ReviewSpecification : BaseSpecification<Review>
{
    public ReviewSpecification(int? userId = null, int? auctionId = null, int? minRating = null)
    {
        // Include navigation properties
        AddInclude(r => r.User);
        AddInclude(r => r.Auction);

        // Apply filters
        if (userId.HasValue)
        {
            And(r => r.UserId == userId);
        }

        if (auctionId.HasValue)
        {
            And(r => r.AuctionId == auctionId);
        }

        if (minRating.HasValue)
        {
            And(r => r.Rating >= minRating);
        }

        // Default ordering by most recent
        ApplyOrderByDescending(r => r.CreatedAt);
    }
}

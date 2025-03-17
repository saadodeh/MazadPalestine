using MzadPalestine.Application.Common.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.Specifications.Favorites;

public class FavoriteSpecification : BaseSpecification<Favorite>
{
    public FavoriteSpecification(int userId, bool? activeAuctionsOnly = null)
    {
        // Include navigation properties
        AddInclude(f => f.User);
        AddInclude(f => f.Auction);

        // Apply user filter
        And(f => f.UserId == userId);

        // Filter for active auctions only
        if (activeAuctionsOnly == true)
        {
            And(f => f.Auction.Status == AuctionStatus.Active &&
                    f.Auction.EndDate > DateTime.UtcNow);
        }

        // Default ordering by most recent
        ApplyOrderByDescending(f => f.CreatedAt);
    }
}

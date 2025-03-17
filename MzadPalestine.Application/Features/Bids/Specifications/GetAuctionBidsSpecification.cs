using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Specifications;

namespace MzadPalestine.Application.Features.Bids.Specifications;

public class GetAuctionBidsSpecification : BaseSpecification<Bid>
{
    public GetAuctionBidsSpecification(
        int auctionId,
        string? sortBy = null,
        bool sortDescending = true,
        int skip = 0,
        int take = 10)
    {
        // Base query with includes
        Criteria = x => x.AuctionId == auctionId;
        AddInclude(x => x.User);
        AddInclude(x => x.Auction);

        // Apply sorting
        switch (sortBy?.ToLower())
        {
            case "amount":
                if (sortDescending)
                    AddOrderByDescending(x => x.Amount);
                else
                    AddOrderBy(x => x.Amount);
                break;

            case "username":
                if (sortDescending)
                    AddOrderByDescending(x => x.User.UserName);
                else
                    AddOrderBy(x => x.User.UserName);
                break;

            default:
                // Default sort by creation date
                if (sortDescending)
                    AddOrderByDescending(x => x.CreatedAt);
                else
                    AddOrderBy(x => x.CreatedAt);
                break;
        }

        // Apply pagination
        ApplyPaging(skip, take);
    }
}

using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Specifications;

namespace MzadPalestine.Application.Features.Auctions.Specifications;

public class GetUserAuctionsSpecification : BaseSpecification<Auction>
{
    public GetUserAuctionsSpecification(
        int userId,
        AuctionStatus? status = null,
        bool onlyBidding = false,
        bool onlySelling = false,
        bool onlyWinning = false,
        string? sortBy = null,
        bool sortDescending = false,
        int skip = 0,
        int take = 10)
    {
        // Base query with includes
        AddInclude(x => x.Seller);
        AddInclude(x => x.Category);
        AddInclude(x => x.Media);
        AddInclude(x => x.Bids);

        // Apply filters
        if (onlySelling)
        {
            Criteria = x => x.SellerId == userId;
        }
        else if (onlyBidding)
        {
            Criteria = x => x.Bids.Any(b => b.UserId == userId);
        }
        else if (onlyWinning)
        {
            Criteria = x => x.Status == AuctionStatus.Sold && 
                           x.Bids.Any(b => b.UserId == userId && b.IsWinning);
        }
        else
        {
            // Default: show both selling and bidding
            Criteria = x => x.SellerId == userId || x.Bids.Any(b => b.UserId == userId);
        }

        if (status.HasValue)
        {
            AndCriteria(x => x.Status == status.Value);
        }

        // Apply sorting
        switch (sortBy?.ToLower())
        {
            case "price":
                if (sortDescending)
                    AddOrderByDescending(x => x.CurrentPrice ?? x.StartingPrice);
                else
                    AddOrderBy(x => x.CurrentPrice ?? x.StartingPrice);
                break;

            case "endtime":
                if (sortDescending)
                    AddOrderByDescending(x => x.EndTime);
                else
                    AddOrderBy(x => x.EndTime);
                break;

            case "bids":
                if (sortDescending)
                    AddOrderByDescending(x => x.Bids.Count);
                else
                    AddOrderBy(x => x.Bids.Count);
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

    private void AndCriteria(System.Linq.Expressions.Expression<Func<Auction, bool>> criteria)
    {
        if (Criteria == null)
            Criteria = criteria;
        else
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Auction));
            var body = System.Linq.Expressions.Expression.AndAlso(
                System.Linq.Expressions.Expression.Invoke(Criteria, parameter),
                System.Linq.Expressions.Expression.Invoke(criteria, parameter)
            );
            Criteria = System.Linq.Expressions.Expression.Lambda<Func<Auction, bool>>(body, parameter);
        }
    }
}

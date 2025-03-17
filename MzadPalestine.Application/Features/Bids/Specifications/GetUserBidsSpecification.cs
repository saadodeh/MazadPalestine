using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Specifications;

namespace MzadPalestine.Application.Features.Bids.Specifications;

public class GetUserBidsSpecification : BaseSpecification<Bid>
{
    public GetUserBidsSpecification(
        int userId,
        bool onlyWinning = false,
        string? sortBy = null,
        bool sortDescending = true,
        int skip = 0,
        int take = 10)
    {
        // Base query with includes
        Criteria = x => x.UserId == userId;
        AddInclude(x => x.User);
        AddInclude(x => x.Auction);

        if (onlyWinning)
        {
            AndCriteria(x => x.IsWinning);
        }

        // Apply sorting
        switch (sortBy?.ToLower())
        {
            case "amount":
                if (sortDescending)
                    AddOrderByDescending(x => x.Amount);
                else
                    AddOrderBy(x => x.Amount);
                break;

            case "auction":
                if (sortDescending)
                    AddOrderByDescending(x => x.Auction.Title);
                else
                    AddOrderBy(x => x.Auction.Title);
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

    private void AndCriteria(System.Linq.Expressions.Expression<Func<Bid, bool>> criteria)
    {
        if (Criteria == null)
            Criteria = criteria;
        else
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Bid));
            var body = System.Linq.Expressions.Expression.AndAlso(
                System.Linq.Expressions.Expression.Invoke(Criteria, parameter),
                System.Linq.Expressions.Expression.Invoke(criteria, parameter)
            );
            Criteria = System.Linq.Expressions.Expression.Lambda<Func<Bid, bool>>(body, parameter);
        }
    }
}

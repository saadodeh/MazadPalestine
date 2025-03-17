using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Specifications;

namespace MzadPalestine.Application.Features.Auctions.Specifications;

public class GetAuctionsSpecification : BaseSpecification<Auction>
{
    public GetAuctionsSpecification(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        AuctionStatus? status = null,
        Currency? currency = null,
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
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            Criteria = x => x.Title.ToLower().Contains(searchTermLower) ||
                           x.Description.ToLower().Contains(searchTermLower);
        }

        if (categoryId.HasValue)
        {
            AndCriteria(x => x.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            AndCriteria(x => x.CurrentPrice >= minPrice.Value || x.StartingPrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            AndCriteria(x => x.CurrentPrice <= maxPrice.Value || x.StartingPrice <= maxPrice.Value);
        }

        if (status.HasValue)
        {
            AndCriteria(x => x.Status == status.Value);
        }

        if (currency.HasValue)
        {
            AndCriteria(x => x.Currency == currency.Value);
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

using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Specifications;

namespace MzadPalestine.Application.Features.Auctions.Specifications;

public class GetAuctionDetailsSpecification : BaseSpecification<Auction>
{
    public GetAuctionDetailsSpecification(int id)
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Seller);
        AddInclude(x => x.Category);
        AddInclude(x => x.Media);
        AddInclude(x => x.Bids);
        AddInclude(x => x.Reviews);
    }
}

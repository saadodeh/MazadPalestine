using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Specifications;

namespace MzadPalestine.Application.Features.Bids.Specifications;

public class GetHighestBidSpecification : BaseSpecification<Bid>
{
    public GetHighestBidSpecification(int auctionId)
        : base(x => x.AuctionId == auctionId)
    {
        AddOrderByDescending(x => x.Amount);
        ApplyPaging(0, 1);
    }
}

using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Bids;

namespace MzadPalestine.Application.Features.Bids.Queries.GetAuctionBids;

public class GetAuctionBidsQuery : IRequest<Result<PaginatedList<BidDto>>>
{
    public int AuctionId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

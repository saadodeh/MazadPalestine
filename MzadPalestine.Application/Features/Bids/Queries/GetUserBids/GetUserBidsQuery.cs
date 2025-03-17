using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Bids;

namespace MzadPalestine.Application.Features.Bids.Queries.GetUserBids;

public class GetUserBidsQuery : IRequest<Result<PaginatedList<BidDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool OnlyWinning { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

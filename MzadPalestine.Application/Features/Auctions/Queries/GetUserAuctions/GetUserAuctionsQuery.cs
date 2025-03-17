using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.Features.Auctions.Queries.GetUserAuctions;

public class GetUserAuctionsQuery : IRequest<Result<PaginatedList<AuctionDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public AuctionStatus? Status { get; init; }
    public bool OnlyBidding { get; init; }
    public bool OnlySelling { get; init; }
    public bool OnlyWinning { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

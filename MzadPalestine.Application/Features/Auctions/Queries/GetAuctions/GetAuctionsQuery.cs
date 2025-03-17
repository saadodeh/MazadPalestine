using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.Features.Auctions.Queries.GetAuctions;

public class GetAuctionsQuery : IRequest<Result<PaginatedList<AuctionDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public AuctionStatus? Status { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public Currency? Currency { get; init; }
}

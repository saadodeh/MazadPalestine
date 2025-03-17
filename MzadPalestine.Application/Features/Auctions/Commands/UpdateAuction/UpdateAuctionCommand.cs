using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.Features.Auctions.Commands.UpdateAuction;

public record UpdateAuctionCommand : IRequest<Result<AuctionDto>>
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public decimal? MinBidIncrement { get; init; }
    public DateTime? EndTime { get; init; }
    public int? CategoryId { get; init; }
    public List<string>? MediaUrls { get; init; }
    public AuctionStatus? Status { get; init; }
}

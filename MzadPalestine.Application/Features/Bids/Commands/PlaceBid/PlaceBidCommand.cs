using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Bids;

namespace MzadPalestine.Application.Features.Bids.Commands.PlaceBid;

public record PlaceBidCommand : IRequest<Result<BidDto>>
{
    public int AuctionId { get; init; }
    public decimal Amount { get; init; }
}

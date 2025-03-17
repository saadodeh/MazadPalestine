using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.Features.Auctions.Commands.CreateAuction;

public record CreateAuctionCommand : IRequest<Result<AuctionDto>>
{
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal StartingPrice { get; init; }
    public decimal MinBidIncrement { get; init; }
    public DateTime EndTime { get; init; }
    public int CategoryId { get; init; }
    public Currency Currency { get; init; }
    public List<string> MediaUrls { get; init; } = new();
}

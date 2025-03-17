using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;

namespace MzadPalestine.Application.Features.Auctions.Commands.CancelAuction;

public record CancelAuctionCommand(int Id) : IRequest<Result<AuctionDto>>;

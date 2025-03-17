using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;

namespace MzadPalestine.Application.Features.Auctions.Commands.EndAuction;

public record EndAuctionCommand(int Id) : IRequest<Result<AuctionDto>>;

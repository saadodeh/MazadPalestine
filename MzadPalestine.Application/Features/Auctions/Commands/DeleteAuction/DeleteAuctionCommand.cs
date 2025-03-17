using MediatR;
using MzadPalestine.Application.Common.Models;

namespace MzadPalestine.Application.Features.Auctions.Commands.DeleteAuction;

public record DeleteAuctionCommand(int Id) : IRequest<Result<Unit>>;

using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;

namespace MzadPalestine.Application.Features.Auctions.Queries.GetAuctionDetails;

public record GetAuctionDetailsQuery(int Id) : IRequest<Result<AuctionDto>>;

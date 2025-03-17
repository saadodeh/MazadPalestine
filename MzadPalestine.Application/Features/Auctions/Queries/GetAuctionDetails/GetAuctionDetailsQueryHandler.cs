using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Exceptions;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Application.Features.Auctions.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Queries.GetAuctionDetails;

public class GetAuctionDetailsQueryHandler : IRequestHandler<GetAuctionDetailsQuery, Result<AuctionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAuctionDetailsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuctionDto>> Handle(GetAuctionDetailsQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetAuctionDetailsSpecification(request.Id);
        var auction = await _unitOfWork.Repository<Auction>().GetEntityWithSpec(spec);

        if (auction == null)
            throw new NotFoundException(nameof(Auction), request.Id);

        var auctionDto = auction.Adapt<AuctionDto>();
        auctionDto.SellerName = auction.Seller.UserName;
        auctionDto.CategoryName = auction.Category.Name;
        auctionDto.BidsCount = auction.Bids.Count;
        auctionDto.MediaUrls = auction.Media.Select(m => m.Url).ToList();

        return Result<AuctionDto>.Success(auctionDto);
    }
}

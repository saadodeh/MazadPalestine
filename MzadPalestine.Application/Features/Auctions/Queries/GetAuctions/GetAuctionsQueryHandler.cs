using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Application.Features.Auctions.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Queries.GetAuctions;

public class GetAuctionsQueryHandler : IRequestHandler<GetAuctionsQuery, Result<PaginatedList<AuctionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAuctionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        
        var spec = new GetAuctionsSpecification(
            searchTerm: request.SearchTerm,
            categoryId: request.CategoryId,
            minPrice: request.MinPrice,
            maxPrice: request.MaxPrice,
            status: request.Status,
            currency: request.Currency,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            skip: skip,
            take: request.PageSize);

        var auctions = await _unitOfWork.Repository<Auction>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<Auction>().CountAsync(spec);

        var auctionDtos = auctions.Select(auction =>
        {
            var dto = auction.Adapt<AuctionDto>();
            dto.SellerName = auction.Seller.UserName;
            dto.CategoryName = auction.Category.Name;
            dto.BidsCount = auction.Bids.Count;
            dto.MediaUrls = auction.Media.Select(m => m.Url).ToList();
            return dto;
        }).ToList();

        var paginatedList = new PaginatedList<AuctionDto>(
            auctionDtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<AuctionDto>>.Success(paginatedList);
    }
}

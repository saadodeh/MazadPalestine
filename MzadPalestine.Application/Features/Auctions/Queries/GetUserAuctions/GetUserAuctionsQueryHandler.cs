using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Application.Features.Auctions.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Queries.GetUserAuctions;

public class GetUserAuctionsQueryHandler : IRequestHandler<GetUserAuctionsQuery, Result<PaginatedList<AuctionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public GetUserAuctionsQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result<PaginatedList<AuctionDto>>> Handle(GetUserAuctionsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<PaginatedList<AuctionDto>>.Failure("User not found");

        var skip = (request.PageNumber - 1) * request.PageSize;
        
        var spec = new GetUserAuctionsSpecification(
            userId: currentUser.Id,
            status: request.Status,
            onlyBidding: request.OnlyBidding,
            onlySelling: request.OnlySelling,
            onlyWinning: request.OnlyWinning,
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
            
            // Add user-specific information
            dto.IsUserBidding = auction.Bids.Any(b => b.UserId == currentUser.Id);
            dto.IsUserWinning = auction.Bids.Any(b => b.UserId == currentUser.Id && b.IsWinning);
            dto.UserHighestBid = auction.Bids
                .Where(b => b.UserId == currentUser.Id)
                .MaxBy(b => b.Amount)?.Amount;
            
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

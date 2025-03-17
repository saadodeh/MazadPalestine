using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Bids;
using MzadPalestine.Application.Features.Bids.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Bids.Queries.GetUserBids;

public class GetUserBidsQueryHandler : IRequestHandler<GetUserBidsQuery, Result<PaginatedList<BidDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public GetUserBidsQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result<PaginatedList<BidDto>>> Handle(GetUserBidsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<PaginatedList<BidDto>>.Failure("User not found");

        var skip = (request.PageNumber - 1) * request.PageSize;
        
        var spec = new GetUserBidsSpecification(
            userId: currentUser.Id,
            onlyWinning: request.OnlyWinning,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            skip: skip,
            take: request.PageSize);

        var bids = await _unitOfWork.Repository<Bid>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<Bid>().CountAsync(spec);

        var bidDtos = bids.Select(bid =>
        {
            var dto = bid.Adapt<BidDto>();
            dto.UserName = bid.User.UserName;
            dto.AuctionTitle = bid.Auction.Title;
            return dto;
        }).ToList();

        var paginatedList = new PaginatedList<BidDto>(
            bidDtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<BidDto>>.Success(paginatedList);
    }
}

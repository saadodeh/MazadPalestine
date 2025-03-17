using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Exceptions;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Bids;
using MzadPalestine.Application.Features.Bids.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Bids.Queries.GetAuctionBids;

public class GetAuctionBidsQueryHandler : IRequestHandler<GetAuctionBidsQuery, Result<PaginatedList<BidDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAuctionBidsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedList<BidDto>>> Handle(GetAuctionBidsQuery request, CancellationToken cancellationToken)
    {
        var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(request.AuctionId);
        if (auction == null)
            throw new NotFoundException(nameof(Auction), request.AuctionId);

        var skip = (request.PageNumber - 1) * request.PageSize;
        
        var spec = new GetAuctionBidsSpecification(
            auctionId: request.AuctionId,
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

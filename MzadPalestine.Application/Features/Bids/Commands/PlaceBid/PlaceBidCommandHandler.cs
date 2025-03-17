using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Exceptions;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Bids;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, Result<BidDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public PlaceBidCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IDomainEventDispatcher eventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<BidDto>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<BidDto>.Failure("User not found");

        var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(request.AuctionId);
        if (auction == null)
            throw new NotFoundException(nameof(Auction), request.AuctionId);

        // Prevent seller from bidding on their own auction
        if (auction.SellerId == currentUser.Id)
            return Result<BidDto>.Failure("You cannot bid on your own auction");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get current highest bid
            var previousHighestBid = await _unitOfWork.Repository<Bid>()
                .GetEntityWithSpec(new GetHighestBidSpecification(request.AuctionId));

            var bid = new Bid
            {
                AuctionId = request.AuctionId,
                UserId = currentUser.Id,
                Amount = request.Amount,
                IsWinning = true,
                CreatedAt = DateTime.UtcNow
            };

            // Mark previous highest bid as not winning
            if (previousHighestBid != null)
            {
                previousHighestBid.IsWinning = false;
                _unitOfWork.Repository<Bid>().Update(previousHighestBid);
            }

            // Update auction's current price
            auction.CurrentPrice = request.Amount;
            auction.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Auction>().Update(auction);

            // Add new bid
            _unitOfWork.Repository<Bid>().Add(bid);

            // Add domain event
            var bidPlacedEvent = new BidPlacedEvent(
                auction.Id,
                currentUser.Id,
                request.Amount,
                true,
                previousHighestBid?.UserId);
            bid.AddDomainEvent(bidPlacedEvent);

            // Save changes
            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // Dispatch events after successful save
            await _eventDispatcher.DispatchAsync(bidPlacedEvent);

            // Map to DTO
            var bidDto = bid.Adapt<BidDto>();
            bidDto.AuctionTitle = auction.Title;
            bidDto.UserName = currentUser.UserName;

            return Result<BidDto>.Success(bidDto);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

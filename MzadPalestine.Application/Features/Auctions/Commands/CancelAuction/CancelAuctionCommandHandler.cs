using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Exceptions;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Application.Features.Auctions.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.CancelAuction;

public class CancelAuctionCommandHandler : IRequestHandler<CancelAuctionCommand, Result<AuctionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CancelAuctionCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IDomainEventDispatcher eventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<AuctionDto>> Handle(CancelAuctionCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<AuctionDto>.Failure("User not found");

        var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(request.Id);
        if (auction == null)
            throw new NotFoundException(nameof(Auction), request.Id);

        // Verify ownership
        if (auction.SellerId != currentUser.Id)
            return Result<AuctionDto>.Failure("You can only cancel your own auctions");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Update auction status
            auction.Status = AuctionStatus.Cancelled;
            auction.UpdatedAt = DateTime.UtcNow;

            // Add domain event
            auction.AddDomainEvent(new AuctionCancelledEvent(
                auction.Id,
                auction.SellerId,
                auction.Title));

            _unitOfWork.Repository<Auction>().Update(auction);

            // Get any active bids for refund processing
            var activeBids = await _unitOfWork.Repository<Bid>()
                .ListAsync(x => x.AuctionId == auction.Id);

            // Mark all bids as cancelled
            foreach (var bid in activeBids)
            {
                bid.IsWinning = false;
                _unitOfWork.Repository<Bid>().Update(bid);

                // Add bid cancelled event for each bid
                bid.AddDomainEvent(new BidCancelledEvent(
                    bid.Id,
                    bid.AuctionId,
                    bid.UserId,
                    bid.Amount));
            }

            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // Dispatch events after successful cancellation
            foreach (var @event in auction.DomainEvents)
            {
                await _eventDispatcher.DispatchAsync(@event);
            }

            foreach (var bid in activeBids)
            {
                foreach (var @event in bid.DomainEvents)
                {
                    await _eventDispatcher.DispatchAsync(@event);
                }
            }

            // Get updated auction with related data
            var updatedAuction = await _unitOfWork.Repository<Auction>()
                .GetEntityWithSpec(new GetAuctionDetailsSpecification(auction.Id));

            var auctionDto = updatedAuction!.Adapt<AuctionDto>();
            auctionDto.SellerName = updatedAuction.Seller.UserName;
            auctionDto.CategoryName = updatedAuction.Category.Name;
            auctionDto.BidsCount = updatedAuction.Bids.Count;
            auctionDto.MediaUrls = updatedAuction.Media.Select(m => m.Url).ToList();

            return Result<AuctionDto>.Success(auctionDto);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

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

namespace MzadPalestine.Application.Features.Auctions.Commands.EndAuction;

public class EndAuctionCommandHandler : IRequestHandler<EndAuctionCommand, Result<AuctionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public EndAuctionCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IDomainEventDispatcher eventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<AuctionDto>> Handle(EndAuctionCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<AuctionDto>.Failure("User not found");

        var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(request.Id);
        if (auction == null)
            throw new NotFoundException(nameof(Auction), request.Id);

        // Verify ownership or admin status
        if (auction.SellerId != currentUser.Id && currentUser.Role != UserRole.Admin)
            return Result<AuctionDto>.Failure("You can only end your own auctions");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get winning bid
            var winningBid = await _unitOfWork.Repository<Bid>()
                .GetEntityWithSpec(x => x.AuctionId == auction.Id && x.IsWinning);

            if (winningBid == null)
                return Result<AuctionDto>.Failure("No winning bid found");

            // Update auction status
            auction.Status = AuctionStatus.Sold;
            auction.UpdatedAt = DateTime.UtcNow;
            auction.CurrentPrice = winningBid.Amount;

            // Create transaction record
            var transaction = new Transaction
            {
                AuctionId = auction.Id,
                BuyerId = winningBid.UserId,
                SellerId = auction.SellerId,
                Amount = winningBid.Amount,
                Status = TransactionStatus.Pending,
                Currency = auction.Currency,
                CreatedAt = DateTime.UtcNow
            };

            // Add domain events
            auction.AddDomainEvent(new AuctionEndedEvent(
                auction.Id,
                auction.SellerId,
                winningBid.UserId,
                auction.Title,
                winningBid.Amount));

            transaction.AddDomainEvent(new TransactionCreatedEvent(
                transaction.Id,
                transaction.AuctionId,
                transaction.BuyerId,
                transaction.SellerId,
                transaction.Amount,
                transaction.Currency));

            // Update database
            _unitOfWork.Repository<Auction>().Update(auction);
            _unitOfWork.Repository<Transaction>().Add(transaction);

            // Create notifications for buyer and seller
            var buyerNotification = new Notification
            {
                UserId = winningBid.UserId,
                Title = "Auction Won!",
                Message = $"Congratulations! You won the auction for {auction.Title} with a bid of {winningBid.Amount} {auction.Currency}",
                Type = NotificationType.AuctionWon,
                CreatedAt = DateTime.UtcNow
            };

            var sellerNotification = new Notification
            {
                UserId = auction.SellerId,
                Title = "Auction Ended",
                Message = $"Your auction for {auction.Title} has ended with a winning bid of {winningBid.Amount} {auction.Currency}",
                Type = NotificationType.AuctionEnded,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Notification>().Add(buyerNotification);
            _unitOfWork.Repository<Notification>().Add(sellerNotification);

            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // Dispatch events after successful completion
            foreach (var @event in auction.DomainEvents.Concat(transaction.DomainEvents))
            {
                await _eventDispatcher.DispatchAsync(@event);
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

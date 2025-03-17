using MediatR;
using MzadPalestine.Application.Common.Exceptions;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.DeleteAuction;

public class DeleteAuctionCommandHandler : IRequestHandler<DeleteAuctionCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public DeleteAuctionCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IDomainEventDispatcher eventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<Unit>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<Unit>.Failure("User not found");

        var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(request.Id);
        if (auction == null)
            throw new NotFoundException(nameof(Auction), request.Id);

        // Verify ownership
        if (auction.SellerId != currentUser.Id)
            return Result<Unit>.Failure("You can only delete your own auctions");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Delete associated media first
            var media = await _unitOfWork.Repository<Media>()
                .ListAsync(x => x.AuctionId == auction.Id);
            
            foreach (var item in media)
            {
                _unitOfWork.Repository<Media>().Delete(item);
            }

            // Add domain event before deletion
            auction.AddDomainEvent(new AuctionDeletedEvent(
                auction.Id,
                auction.SellerId,
                auction.Title));

            // Delete the auction
            _unitOfWork.Repository<Auction>().Delete(auction);
            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // Dispatch events after successful deletion
            foreach (var @event in auction.DomainEvents)
            {
                await _eventDispatcher.DispatchAsync(@event);
            }

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

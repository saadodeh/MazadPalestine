using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Exceptions;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.UpdateAuction;

public class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand, Result<AuctionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public UpdateAuctionCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IDomainEventDispatcher eventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<AuctionDto>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<AuctionDto>.Failure("User not found");

        var auction = await _unitOfWork.Repository<Auction>().GetByIdAsync(request.Id);
        if (auction == null)
            throw new NotFoundException(nameof(Auction), request.Id);

        // Verify ownership
        if (auction.SellerId != currentUser.Id)
            return Result<AuctionDto>.Failure("You can only update your own auctions");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Update basic properties
            if (request.Title != null)
                auction.Title = request.Title;

            if (request.Description != null)
                auction.Description = request.Description;

            if (request.MinBidIncrement.HasValue)
                auction.MinBidIncrement = request.MinBidIncrement.Value;

            if (request.EndTime.HasValue)
                auction.EndTime = request.EndTime.Value;

            if (request.CategoryId.HasValue)
                auction.CategoryId = request.CategoryId.Value;

            if (request.Status.HasValue)
                auction.Status = request.Status.Value;

            // Update media if provided
            if (request.MediaUrls != null)
            {
                // Remove existing media
                var existingMedia = await _unitOfWork.Repository<Media>()
                    .ListAsync(x => x.AuctionId == auction.Id);
                
                foreach (var media in existingMedia)
                {
                    _unitOfWork.Repository<Media>().Delete(media);
                }

                // Add new media
                foreach (var url in request.MediaUrls)
                {
                    auction.Media.Add(new Media
                    {
                        Url = url,
                        Type = Core.Enums.MediaType.Image,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            auction.UpdatedAt = DateTime.UtcNow;

            // Add domain event
            auction.AddDomainEvent(new AuctionUpdatedEvent(
                auction.Id,
                auction.SellerId,
                auction.Title,
                auction.Status));

            _unitOfWork.Repository<Auction>().Update(auction);
            await _unitOfWork.CompleteAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // Dispatch events after successful save
            foreach (var @event in auction.DomainEvents)
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

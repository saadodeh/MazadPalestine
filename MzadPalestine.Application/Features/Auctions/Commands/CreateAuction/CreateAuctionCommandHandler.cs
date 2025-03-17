using Mapster;
using MediatR;
using MzadPalestine.Application.Common.Models;
using MzadPalestine.Application.DTOs.Auctions;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Events;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.CreateAuction;

public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, Result<AuctionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CreateAuctionCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IDomainEventDispatcher eventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _identityService.GetCurrentUserAsync();
        if (currentUser == null)
            return Result<AuctionDto>.Failure("User not found");

        var auction = new Auction
        {
            Title = request.Title,
            Description = request.Description,
            StartingPrice = request.StartingPrice,
            MinBidIncrement = request.MinBidIncrement,
            EndTime = request.EndTime,
            SellerId = currentUser.Id,
            CategoryId = request.CategoryId,
            Currency = request.Currency,
            CreatedAt = DateTime.UtcNow
        };

        // Add media
        foreach (var url in request.MediaUrls)
        {
            auction.Media.Add(new Media
            {
                Url = url,
                Type = Core.Enums.MediaType.Image,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Add domain event
        auction.AddDomainEvent(new AuctionCreatedEvent(
            auction.Id,
            auction.SellerId,
            auction.Title,
            auction.StartingPrice,
            auction.EndTime));

        _unitOfWork.Repository<Auction>().Add(auction);
        await _unitOfWork.CompleteAsync();

        // Dispatch events after successful save
        foreach (var @event in auction.DomainEvents)
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        var auctionDto = auction.Adapt<AuctionDto>();
        auctionDto.SellerName = currentUser.UserName;
        
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(auction.CategoryId);
        if (category != null)
        {
            auctionDto.CategoryName = category.Name;
        }

        return Result<AuctionDto>.Success(auctionDto);
    }
}

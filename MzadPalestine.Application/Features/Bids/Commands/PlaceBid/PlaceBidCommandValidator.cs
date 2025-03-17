using FluentValidation;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
{
    private readonly IGenericRepository<Auction> _auctionRepository;
    private readonly IGenericRepository<Bid> _bidRepository;

    public PlaceBidCommandValidator(
        IGenericRepository<Auction> auctionRepository,
        IGenericRepository<Bid> bidRepository)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;

        RuleFor(x => x.AuctionId)
            .MustAsync(async (id, cancellation) =>
            {
                var auction = await _auctionRepository.GetByIdAsync(id);
                return auction != null && 
                       auction.Status == AuctionStatus.Active && 
                       auction.EndTime > DateTime.UtcNow;
            })
            .WithMessage("Auction not found or not active");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Bid amount must be greater than 0")
            .MustAsync(async (command, amount, cancellation) =>
            {
                var auction = await _auctionRepository.GetByIdAsync(command.AuctionId);
                if (auction == null) return false;

                var highestBid = await _bidRepository.GetEntityWithSpec(
                    new GetHighestBidSpecification(command.AuctionId));

                // If no bids yet, check against starting price
                if (highestBid == null)
                {
                    return amount >= auction.StartingPrice;
                }

                // Otherwise, check against highest bid + minimum increment
                return amount >= (highestBid.Amount + auction.MinBidIncrement);
            })
            .WithMessage("Bid amount must be greater than or equal to the minimum required bid");
    }
}

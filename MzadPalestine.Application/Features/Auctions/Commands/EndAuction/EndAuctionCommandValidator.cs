using FluentValidation;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.EndAuction;

public class EndAuctionCommandValidator : AbstractValidator<EndAuctionCommand>
{
    private readonly IGenericRepository<Auction> _auctionRepository;

    public EndAuctionCommandValidator(IGenericRepository<Auction> auctionRepository)
    {
        _auctionRepository = auctionRepository;

        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellation) =>
            {
                var auction = await _auctionRepository.GetByIdAsync(id);
                if (auction == null)
                    return false;

                // Can only end active auctions
                if (auction.Status != AuctionStatus.Active)
                    return false;

                // Can only end if there's at least one bid
                if (!await _auctionRepository.AnyAsync(a => a.Id == id && a.Bids.Any()))
                    return false;

                // Can only end if the auction end time has passed or is within 5 minutes
                return auction.EndTime <= DateTime.UtcNow.AddMinutes(5);
            })
            .WithMessage("Cannot end this auction. Auction must be active, have at least one bid, and be near or past its end time");
    }
}

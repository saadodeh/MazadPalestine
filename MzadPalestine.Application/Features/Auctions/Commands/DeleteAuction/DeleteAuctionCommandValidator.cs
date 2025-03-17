using FluentValidation;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.DeleteAuction;

public class DeleteAuctionCommandValidator : AbstractValidator<DeleteAuctionCommand>
{
    private readonly IGenericRepository<Auction> _auctionRepository;

    public DeleteAuctionCommandValidator(IGenericRepository<Auction> auctionRepository)
    {
        _auctionRepository = auctionRepository;

        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellation) =>
            {
                var auction = await _auctionRepository.GetByIdAsync(id);
                if (auction == null)
                    return false;

                // Can't delete auctions that have bids or are already sold
                return auction.Status != AuctionStatus.Sold && 
                       !await _auctionRepository.AnyAsync(b => b.Id == id && b.Bids.Any());
            })
            .WithMessage("Cannot delete auction that has bids or is already sold");
    }
}

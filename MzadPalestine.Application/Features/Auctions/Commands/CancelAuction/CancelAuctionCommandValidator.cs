using FluentValidation;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.CancelAuction;

public class CancelAuctionCommandValidator : AbstractValidator<CancelAuctionCommand>
{
    private readonly IGenericRepository<Auction> _auctionRepository;

    public CancelAuctionCommandValidator(IGenericRepository<Auction> auctionRepository)
    {
        _auctionRepository = auctionRepository;

        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellation) =>
            {
                var auction = await _auctionRepository.GetByIdAsync(id);
                if (auction == null)
                    return false;

                // Can only cancel active auctions
                return auction.Status == AuctionStatus.Active;
            })
            .WithMessage("Can only cancel active auctions");
    }
}

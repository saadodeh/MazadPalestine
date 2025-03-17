using FluentValidation;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.UpdateAuction;

public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IGenericRepository<Auction> _auctionRepository;

    public UpdateAuctionCommandValidator(
        IGenericRepository<Category> categoryRepository,
        IGenericRepository<Auction> auctionRepository)
    {
        _categoryRepository = categoryRepository;
        _auctionRepository = auctionRepository;

        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellation) =>
            {
                var auction = await _auctionRepository.GetByIdAsync(id);
                return auction != null && auction.Status != AuctionStatus.Sold;
            })
            .WithMessage("Auction not found or already sold");

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .MinimumLength(3)
                .MaximumLength(100);
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MinimumLength(10)
                .MaximumLength(2000);
        });

        When(x => x.MinBidIncrement.HasValue, () =>
        {
            RuleFor(x => x.MinBidIncrement!.Value)
                .GreaterThan(0)
                .WithMessage("Minimum bid increment must be greater than 0")
                .MustAsync(async (command, minBidIncrement, cancellation) =>
                {
                    var auction = await _auctionRepository.GetByIdAsync(command.Id);
                    return auction != null && minBidIncrement < auction.StartingPrice;
                })
                .WithMessage("Minimum bid increment must be less than starting price");
        });

        When(x => x.EndTime.HasValue, () =>
        {
            RuleFor(x => x.EndTime!.Value)
                .GreaterThan(DateTime.UtcNow.AddHours(1))
                .WithMessage("Auction end time must be at least 1 hour in the future")
                .LessThan(DateTime.UtcNow.AddDays(30))
                .WithMessage("Auction cannot last longer than 30 days");
        });

        When(x => x.CategoryId.HasValue, () =>
        {
            RuleFor(x => x.CategoryId!.Value)
                .MustAsync(async (id, cancellation) =>
                {
                    var category = await _categoryRepository.GetByIdAsync(id);
                    return category != null;
                })
                .WithMessage("Invalid category");
        });

        When(x => x.MediaUrls != null, () =>
        {
            RuleFor(x => x.MediaUrls)
                .Must(x => x!.Count <= 10)
                .WithMessage("Maximum 10 images allowed");
        });

        When(x => x.Status.HasValue, () =>
        {
            RuleFor(x => x.Status!.Value)
                .IsInEnum()
                .MustAsync(async (command, status, cancellation) =>
                {
                    if (status == AuctionStatus.Sold)
                        return false;

                    var auction = await _auctionRepository.GetByIdAsync(command.Id);
                    if (auction == null)
                        return false;

                    // Can't reactivate a cancelled auction
                    if (auction.Status == AuctionStatus.Cancelled && status == AuctionStatus.Active)
                        return false;

                    return true;
                })
                .WithMessage("Invalid status transition");
        });
    }
}

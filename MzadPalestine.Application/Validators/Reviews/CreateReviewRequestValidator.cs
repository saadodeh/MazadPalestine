using FluentValidation;
using MzadPalestine.Application.DTOs.Reviews;
using MzadPalestine.Application.Interfaces.Repositories;

namespace MzadPalestine.Application.Validators.Reviews;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IReviewRepository _reviewRepository;

    public CreateReviewRequestValidator(
        IAuctionRepository auctionRepository,
        IReviewRepository reviewRepository)
    {
        _auctionRepository = auctionRepository;
        _reviewRepository = reviewRepository;

        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction ID is required")
            .MustAsync(async (auctionId, cancellation) =>
            {
                var exists = await _auctionRepository.AnyAsync(a => a.Id == auctionId);
                return exists;
            }).WithMessage("Auction does not exist")
            .MustAsync(async (auctionId, cancellation) =>
            {
                var auction = await _auctionRepository.GetByIdAsync(auctionId);
                return auction?.Status == Core.Enums.AuctionStatus.Sold;
            }).WithMessage("Can only review completed auctions")
            .MustAsync(async (request, auctionId, context) =>
            {
                // Get the current user ID from the context
                var userId = context.RootContextData["CurrentUserId"] as int?;
                if (!userId.HasValue) return false;

                var exists = await _reviewRepository.AnyAsync(r => 
                    r.AuctionId == auctionId && r.UserId == userId.Value);
                return !exists;
            }).WithMessage("You have already reviewed this auction");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("Comment cannot exceed 1000 characters");
    }
}

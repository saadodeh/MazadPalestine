using FluentValidation;
using MzadPalestine.Core.Interfaces;

namespace MzadPalestine.Application.Features.Auctions.Commands.CreateAuction;

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    private readonly IGenericRepository<Core.Entities.Category> _categoryRepository;

    public CreateAuctionCommandValidator(IGenericRepository<Core.Entities.Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(2000);

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0)
            .WithMessage("Starting price must be greater than 0");

        RuleFor(x => x.MinBidIncrement)
            .GreaterThan(0)
            .WithMessage("Minimum bid increment must be greater than 0")
            .LessThan(x => x.StartingPrice)
            .WithMessage("Minimum bid increment must be less than starting price");

        RuleFor(x => x.EndTime)
            .GreaterThan(DateTime.UtcNow.AddHours(1))
            .WithMessage("Auction end time must be at least 1 hour in the future")
            .LessThan(DateTime.UtcNow.AddDays(30))
            .WithMessage("Auction cannot last longer than 30 days");

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, cancellation) =>
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                return category != null;
            })
            .WithMessage("Invalid category");

        RuleFor(x => x.MediaUrls)
            .NotEmpty()
            .WithMessage("At least one image is required")
            .Must(x => x.Count <= 10)
            .WithMessage("Maximum 10 images allowed");
    }
}

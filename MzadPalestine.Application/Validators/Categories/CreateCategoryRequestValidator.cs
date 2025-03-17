using FluentValidation;
using MzadPalestine.Application.DTOs.Categories;
using MzadPalestine.Application.Interfaces.Repositories;

namespace MzadPalestine.Application.Validators.Categories;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryRequestValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters")
            .MustAsync(async (name, cancellation) =>
            {
                var exists = await _categoryRepository.AnyAsync(c => c.Name == name);
                return !exists;
            }).WithMessage("A category with this name already exists");

        When(x => x.ParentId.HasValue, () =>
        {
            RuleFor(x => x.ParentId)
                .MustAsync(async (parentId, cancellation) =>
                {
                    var exists = await _categoryRepository.AnyAsync(c => c.Id == parentId);
                    return exists;
                }).WithMessage("Parent category does not exist");
        });
    }
}

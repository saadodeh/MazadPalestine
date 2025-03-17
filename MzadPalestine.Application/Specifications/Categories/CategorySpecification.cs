using MzadPalestine.Application.Common.Specifications;
using MzadPalestine.Core.Entities;

namespace MzadPalestine.Application.Specifications.Categories;

public class CategorySpecification : BaseSpecification<Category>
{
    public CategorySpecification(string? searchTerm = null, int? parentId = null)
    {
        // Include navigation properties
        AddInclude(c => c.Parent);
        AddInclude(c => c.Children);
        AddInclude(c => c.Auctions);

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            And(c => c.Name.Contains(searchTerm));
        }

        if (parentId.HasValue)
        {
            And(c => c.ParentId == parentId);
        }

        // Default ordering
        ApplyOrderBy(c => c.Name);
    }
}

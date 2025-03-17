namespace MzadPalestine.Application.DTOs.Categories;

public class CreateCategoryRequest
{
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
}

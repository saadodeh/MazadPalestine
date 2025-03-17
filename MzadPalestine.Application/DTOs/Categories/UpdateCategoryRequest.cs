namespace MzadPalestine.Application.DTOs.Categories;

public class UpdateCategoryRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
}

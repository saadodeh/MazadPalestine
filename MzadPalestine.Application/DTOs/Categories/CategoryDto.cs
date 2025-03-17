namespace MzadPalestine.Application.DTOs.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public CategoryDto? Parent { get; set; }
    public ICollection<CategoryDto> Children { get; set; } = new List<CategoryDto>();
}

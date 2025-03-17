using MzadPalestine.Core.Common;

namespace MzadPalestine.Core.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    
    // Navigation properties
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}

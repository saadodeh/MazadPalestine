namespace MzadPalestine.Application.DTOs.Favorites;

public class FavoriteDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int AuctionId { get; set; }
    public string AuctionTitle { get; set; } = null!;
    public string? AuctionThumbnail { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime AuctionEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

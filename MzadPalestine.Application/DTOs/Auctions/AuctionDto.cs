using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.DTOs.Auctions;

public class AuctionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal StartingPrice { get; set; }
    public decimal MinBidIncrement { get; set; }
    public decimal? CurrentPrice { get; set; }
    public DateTime EndTime { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public AuctionStatus Status { get; set; }
    public Currency Currency { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int BidsCount { get; set; }
    public List<string> MediaUrls { get; set; } = new();
}

namespace MzadPalestine.Application.DTOs.Bids;

public class BidDto
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public string AuctionTitle { get; set; } = null!;
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public decimal Amount { get; set; }
    public bool IsWinning { get; set; }
    public DateTime CreatedAt { get; set; }
}

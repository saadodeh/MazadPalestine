using MzadPalestine.Core.Common;

namespace MzadPalestine.Core.Entities;

public class Bid : BaseEntity
{
    public int AuctionId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public bool IsWinning { get; set; }

    // Navigation properties
    public Auction Auction { get; set; } = null!;
    public User User { get; set; } = null!;
}

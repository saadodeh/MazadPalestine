using MzadPalestine.Core.Common;

namespace MzadPalestine.Core.Entities;

public class Favorite : BaseEntity
{
    public int UserId { get; set; }
    public int AuctionId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Auction Auction { get; set; } = null!;
}

using MzadPalestine.Core.Common;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Entities;

public class Media : BaseEntity
{
    public int? AuctionId { get; set; }
    public int? UserId { get; set; }
    public string Url { get; set; } = null!;
    public MediaType Type { get; set; }
    public long Size { get; set; }

    // Navigation properties
    public Auction? Auction { get; set; }
    public User? User { get; set; }
}

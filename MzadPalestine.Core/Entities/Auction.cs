using MzadPalestine.Core.Common;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Entities;

public class Auction : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal StartingPrice { get; set; }
    public decimal MinBidIncrement { get; set; }
    public DateTime EndTime { get; set; }
    public decimal? CurrentPrice { get; set; }
    public int SellerId { get; set; }
    public int CategoryId { get; set; }
    public AuctionStatus Status { get; set; } = AuctionStatus.Active;
    public Currency Currency { get; set; } = Currency.ILS;

    // Navigation properties
    public User Seller { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<Media> Media { get; set; } = new List<Media>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}

using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.DTOs.Auctions;

public class CreateAuctionDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal StartingPrice { get; set; }
    public decimal MinBidIncrement { get; set; }
    public DateTime EndTime { get; set; }
    public int CategoryId { get; set; }
    public Currency Currency { get; set; } = Currency.ILS;
    public List<string> MediaUrls { get; set; } = new();
}

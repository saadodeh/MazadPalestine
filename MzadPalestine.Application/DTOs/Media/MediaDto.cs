using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.DTOs.Media;

public class MediaDto
{
    public int Id { get; set; }
    public int? AuctionId { get; set; }
    public int? UserId { get; set; }
    public string Url { get; set; } = null!;
    public MediaType Type { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

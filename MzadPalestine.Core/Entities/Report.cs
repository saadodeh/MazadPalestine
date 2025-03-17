using MzadPalestine.Core.Common;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Entities;

public class Report : BaseEntity
{
    public int ReportedByUserId { get; set; }
    public int? ReportedUserId { get; set; }
    public int? ReportedAuctionId { get; set; }
    public string Reason { get; set; } = null!;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? AdminResponse { get; set; }

    // Navigation properties
    public User ReportedByUser { get; set; } = null!;
    public User? ReportedUser { get; set; }
    public Auction? ReportedAuction { get; set; }
}

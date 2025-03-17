using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.DTOs.Reports;

public class ReportDto
{
    public int Id { get; set; }
    public int ReportedByUserId { get; set; }
    public string ReportedByUserName { get; set; } = null!;
    public int? ReportedUserId { get; set; }
    public string? ReportedUserName { get; set; }
    public int? ReportedAuctionId { get; set; }
    public string? ReportedAuctionTitle { get; set; }
    public string Reason { get; set; } = null!;
    public ReportStatus Status { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

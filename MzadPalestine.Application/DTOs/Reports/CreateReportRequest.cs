namespace MzadPalestine.Application.DTOs.Reports;

public class CreateReportRequest
{
    public int? ReportedUserId { get; set; }
    public int? ReportedAuctionId { get; set; }
    public string Reason { get; set; } = null!;
}

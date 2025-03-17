using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.DTOs.Reports;

public class AdminReportResponse
{
    public int Id { get; set; }
    public ReportStatus Status { get; set; }
    public string? AdminResponse { get; set; }
}

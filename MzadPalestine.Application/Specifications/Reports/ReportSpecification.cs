using MzadPalestine.Application.Common.Specifications;
using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.Specifications.Reports;

public class ReportSpecification : BaseSpecification<Report>
{
    public ReportSpecification(int? reportedByUserId = null, int? reportedUserId = null, 
        int? reportedAuctionId = null, ReportStatus? status = null)
    {
        // Include navigation properties
        AddInclude(r => r.ReportedByUser);
        AddInclude(r => r.ReportedUser);
        AddInclude(r => r.ReportedAuction);

        // Apply filters
        if (reportedByUserId.HasValue)
        {
            And(r => r.ReportedByUserId == reportedByUserId);
        }

        if (reportedUserId.HasValue)
        {
            And(r => r.ReportedUserId == reportedUserId);
        }

        if (reportedAuctionId.HasValue)
        {
            And(r => r.ReportedAuctionId == reportedAuctionId);
        }

        if (status.HasValue)
        {
            And(r => r.Status == status);
        }

        // Default ordering by most recent
        ApplyOrderByDescending(r => r.CreatedAt);
    }
}

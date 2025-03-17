using MzadPalestine.Core.Common;
using MzadPalestine.Core.Entities;

namespace MzadPalestine.Application.Features.Notifications.Specifications;

public class GetNotificationByIdSpecification : BaseSpecification<Notification>
{
    public GetNotificationByIdSpecification(int id)
    {
        // Base query
        Criteria = n => n.Id == id;

        // Include related data
        AddInclude(n => n.User);
    }
}

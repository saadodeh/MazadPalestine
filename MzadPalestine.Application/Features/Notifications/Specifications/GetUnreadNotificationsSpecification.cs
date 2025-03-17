using MzadPalestine.Core.Common;
using MzadPalestine.Core.Entities;

namespace MzadPalestine.Application.Features.Notifications.Specifications;

public class GetUnreadNotificationsSpecification : BaseSpecification<Notification>
{
    public GetUnreadNotificationsSpecification(int userId)
    {
        // Base query for unread notifications
        Criteria = n => n.UserId == userId && !n.IsRead;

        // Order by creation date, newest first
        AddOrderByDescending(n => n.CreatedAt);
    }
}

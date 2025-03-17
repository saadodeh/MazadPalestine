using MzadPalestine.Core.Common;
using MzadPalestine.Core.Entities;

namespace MzadPalestine.Application.Features.Notifications.Specifications;

public class GetUserNotificationsSpecification : BaseSpecification<Notification>
{
    public GetUserNotificationsSpecification(
        int userId,
        bool? isRead = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = true)
    {
        // Base query
        Criteria = n => n.UserId == userId;

        // Apply filters
        if (isRead.HasValue)
            AndAlso(n => n.IsRead == isRead.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            AndAlso(n => n.Title.Contains(searchTerm) || n.Message.Contains(searchTerm));

        // Apply sorting
        switch (sortBy?.ToLower())
        {
            case "createdat":
                if (sortDescending)
                    AddOrderByDescending(n => n.CreatedAt);
                else
                    AddOrderBy(n => n.CreatedAt);
                break;

            case "readat":
                if (sortDescending)
                    AddOrderByDescending(n => n.ReadAt);
                else
                    AddOrderBy(n => n.ReadAt);
                break;

            case "type":
                if (sortDescending)
                    AddOrderByDescending(n => n.Type);
                else
                    AddOrderBy(n => n.Type);
                break;

            default:
                AddOrderByDescending(n => n.CreatedAt);
                break;
        }
    }
}

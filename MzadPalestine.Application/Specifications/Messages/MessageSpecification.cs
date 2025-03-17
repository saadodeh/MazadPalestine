using MzadPalestine.Application.Common.Specifications;
using MzadPalestine.Core.Entities;

namespace MzadPalestine.Application.Specifications.Messages;

public class MessageSpecification : BaseSpecification<Message>
{
    public MessageSpecification(int userId, int? otherUserId = null, bool? unreadOnly = null)
    {
        // Include navigation properties
        AddInclude(m => m.Sender);
        AddInclude(m => m.Receiver);

        // Apply filters for user's messages (sent or received)
        And(m => m.SenderId == userId || m.ReceiverId == userId);

        if (otherUserId.HasValue)
        {
            And(m => (m.SenderId == otherUserId && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == otherUserId));
        }

        if (unreadOnly == true)
        {
            And(m => !m.IsRead && m.ReceiverId == userId);
        }

        // Default ordering by most recent
        ApplyOrderByDescending(m => m.CreatedAt);
    }
}

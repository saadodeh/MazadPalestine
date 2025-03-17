using MzadPalestine.Core.Entities;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(
        int userId,
        string message,
        NotificationType type);

    Task SendBidNotificationAsync(
        int auctionId,
        int bidderId,
        decimal bidAmount);

    Task SendAuctionEndedNotificationAsync(
        int auctionId,
        int? winnerId,
        decimal finalPrice);

    Task SendMessageNotificationAsync(
        int senderId,
        int receiverId,
        string messagePreview);

    Task MarkNotificationAsReadAsync(int notificationId);
    
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        int userId,
        bool includeRead = false);
        
    Task<int> GetUnreadNotificationCountAsync(int userId);
}

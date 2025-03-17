using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.DTOs.Settings;

public class SettingsDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public Language Language { get; set; }
    public bool DarkMode { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

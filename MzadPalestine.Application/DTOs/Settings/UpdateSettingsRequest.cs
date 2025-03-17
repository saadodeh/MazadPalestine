using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.DTOs.Settings;

public class UpdateSettingsRequest
{
    public Language Language { get; set; }
    public bool DarkMode { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
}

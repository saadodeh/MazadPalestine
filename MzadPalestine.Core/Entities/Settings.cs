using MzadPalestine.Core.Common;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Entities;

public class Settings : BaseEntity
{
    public int UserId { get; set; }
    public Language Language { get; set; } = Language.Arabic;
    public bool DarkMode { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}

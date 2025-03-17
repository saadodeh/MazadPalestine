using MzadPalestine.Core.Enums;

namespace MzadPalestine.Application.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfilePicture { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ActiveAuctionsCount { get; set; }
    public int CompletedAuctionsCount { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalSpent { get; set; }
    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }
}

using MzadPalestine.Core.Entities;

namespace MzadPalestine.Core.Interfaces;

public interface IIdentityService
{
    Task<(bool Success, string Token)> AuthenticateAsync(
        string email,
        string password);

    Task<(bool Success, string[] Errors)> RegisterAsync(
        string email,
        string userName,
        string password,
        string? phoneNumber = null);

    Task<bool> ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword);

    Task<(bool Success, string Token)> RefreshTokenAsync(
        string refreshToken);

    Task<bool> RevokeTokenAsync(string token);

    Task<bool> AssignRoleAsync(int userId, string role);
    
    Task<IList<string>> GetUserRolesAsync(int userId);
    
    Task<bool> ValidateTokenAsync(string token);
    
    Task<User?> GetCurrentUserAsync();
    
    Task<bool> IsInRoleAsync(int userId, string role);
}

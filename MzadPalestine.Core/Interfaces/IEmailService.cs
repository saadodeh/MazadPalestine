namespace MzadPalestine.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true);

    Task SendWelcomeEmailAsync(
        string to,
        string userName);

    Task SendPasswordResetEmailAsync(
        string to,
        string resetToken,
        string userName);

    Task SendAuctionWonEmailAsync(
        string to,
        string userName,
        string auctionTitle,
        decimal finalPrice);

    Task SendBidOutbidEmailAsync(
        string to,
        string userName,
        string auctionTitle,
        decimal newBidAmount);

    Task SendAuctionEndingSoonEmailAsync(
        string to,
        string userName,
        string auctionTitle,
        DateTime endTime);
}

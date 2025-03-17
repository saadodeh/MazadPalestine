namespace MzadPalestine.Application.DTOs.Messages;

public class MessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string? SenderProfilePicture { get; set; }
    public int ReceiverId { get; set; }
    public string ReceiverName { get; set; } = null!;
    public string? ReceiverProfilePicture { get; set; }
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

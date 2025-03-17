namespace MzadPalestine.Application.DTOs.Messages;

public class SendMessageRequest
{
    public int ReceiverId { get; set; }
    public string Content { get; set; } = null!;
}

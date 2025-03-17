namespace MzadPalestine.Application.DTOs.Reviews;

public class CreateReviewRequest
{
    public int AuctionId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

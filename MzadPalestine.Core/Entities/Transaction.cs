using MzadPalestine.Core.Common;
using MzadPalestine.Core.Enums;

namespace MzadPalestine.Core.Entities;

public class Transaction : BaseEntity
{
    public int UserId { get; set; }
    public int? AuctionId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionStatus Status { get; set; }
    public string? TransactionReference { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Auction? Auction { get; set; }
}

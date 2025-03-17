namespace MzadPalestine.Core.Enums;

public enum NotificationType
{
    // Auction related
    AuctionCreated,
    AuctionUpdated,
    AuctionEnded,
    AuctionCancelled,
    AuctionDeleted,
    AuctionWon,
    
    // Bid related
    BidPlaced,
    BidReceived,
    BidCancelled,
    Outbid,
    
    // Transaction related
    TransactionCreated,
    PaymentRequired,
    PaymentReceived,
    PaymentSent,
    
    // General
    Message,
    NewReview,
    SystemNotification
}

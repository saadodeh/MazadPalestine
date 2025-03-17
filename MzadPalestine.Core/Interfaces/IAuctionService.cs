using MzadPalestine.Core.Entities;

namespace MzadPalestine.Core.Interfaces;

public interface IAuctionService
{
    Task<Auction> CreateAuctionAsync(Auction auction);
    Task<bool> PlaceBidAsync(int auctionId, int userId, decimal amount);
    Task<bool> EndAuctionAsync(int auctionId);
    Task<IEnumerable<Auction>> GetActiveAuctionsAsync();
    Task<IEnumerable<Auction>> GetUserAuctionsAsync(int userId);
    Task<IEnumerable<Bid>> GetAuctionBidsAsync(int auctionId);
    Task<decimal> GetRecommendedStartingPriceAsync(string category, string condition);
    Task<bool> CancelAuctionAsync(int auctionId, int userId);
    Task<bool> ExtendAuctionTimeAsync(int auctionId, TimeSpan extension);
}

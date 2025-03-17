using MzadPalestine.Core.Entities;

namespace MzadPalestine.Core.Interfaces;

public interface IFileStorageService
{
    Task<Media> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        int? auctionId = null,
        int? userId = null);

    Task<bool> DeleteFileAsync(string fileUrl);
    
    Task<Stream> GetFileAsync(string fileUrl);
    
    Task<IEnumerable<Media>> GetAuctionMediaAsync(int auctionId);
    
    Task<IEnumerable<Media>> GetUserMediaAsync(int userId);
    
    Task<bool> ValidateFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long maxSizeInBytes = 10485760); // Default 10MB
}

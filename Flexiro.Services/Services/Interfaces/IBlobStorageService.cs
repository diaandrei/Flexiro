namespace Flexiro.Services.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName);
        Task<Stream> GetImageAsync(string fileName);
        Task DeleteImageAsync(string fileName);
        Task<List<string>> GetProductImageUrlsAsync(int productId);
        string GetImageUrl(string relativePath);
    }
}
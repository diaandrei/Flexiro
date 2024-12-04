using Flexiro.Application.Models;

namespace Flexiro.Services.Repositories
{
    public interface IReviewRepository
    {
        Task<bool> ProductExistsAsync(int productId);
        Task<Review> GetExistingReviewAsync(int productId, string userId);
        Task AddReviewAsync(Review review);
        Task UpdateReviewAsync(Review review);
        Task<Review> GetReviewByIdAsync(int reviewId);
        Task DeleteReviewAsync(Review review);
        Task<IList<Review>> GetReviewsByProductIdAsync(int productId);
        Task<IList<Review>> GetReviewsByUserIdAsync(string userId);
        Task<IList<Product>> GetProductsByShopIdAsync(int shopId);
        Task<IList<Review>> GetReviewsByProductIdsAsync(IList<int> productIds);
    }
}
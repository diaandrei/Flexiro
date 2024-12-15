using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ResponseModel<ReviewResponseDto>> AddOrUpdateReviewAsync(AddReviewRequestDto review);
        Task<ResponseModel<string>> DeleteReviewAsync(int reviewId);
        Task<ResponseModel<Review>> GetReviewByIdAsync(int reviewId);
        Task<ResponseModel<IList<Review>>> GetReviewsByProductIdAsync(int productId);
        Task<ResponseModel<IList<Review>>> GetReviewsByUserIdAsync(string userId);
        Task<ResponseModel<string>> GetAverageRatingAsync(int productId);
        Task<ResponseModel<string>> GetAverageRatingByShopIdAsync(int shopId);
    }
}
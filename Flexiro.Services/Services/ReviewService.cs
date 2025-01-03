using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Flexiro.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;
        private readonly IReviewRepository _reviewRepository;
        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReviewService> logger, IReviewRepository reviewRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _reviewRepository = reviewRepository;
        }
        public async Task<ResponseModel<ReviewResponseDto>> AddOrUpdateReviewAsync(AddReviewRequestDto review)
        {
            var response = new ResponseModel<ReviewResponseDto>();

            try
            {
                // Validate rating
                if (review.Rating is < 1 or > 5)
                {
                    response.Success = false;
                    response.Title = "Invalid Rating";
                    response.Description = "Rating must be between 1 and 5.";
                    return response;
                }

                // Check if the product exists
                if (!await _reviewRepository.ProductExistsAsync(review.ProductId))
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{review.ProductId}' does not exist.";
                    return response;
                }

                // Check for an existing review
                var existingReview = await _reviewRepository.GetExistingReviewAsync(review.ProductId, review.UserId);

                if (existingReview != null!)
                {
                    // Update existing review
                    existingReview.Rating = review.Rating;
                    await _reviewRepository.UpdateReviewAsync(existingReview);

                    response.Success = true;
                    response.Title = "Review Updated Successfully";
                    response.Description = "Your review has been updated.";
                    response.Content = new ReviewResponseDto
                    {
                        ReviewId = existingReview.ReviewId,
                        Rating = existingReview.Rating ?? 0,
                        ProductId = existingReview.ProductId,
                        UserId = existingReview.UserId,
                        UserName = existingReview.User?.UserName!
                    };
                }
                else
                {
                    // Add new review
                    var newReview = new Review
                    {
                        ProductId = review.ProductId,
                        Rating = review.Rating,
                        UserId = review.UserId
                    };

                    await _reviewRepository.AddReviewAsync(newReview);

                    response.Success = true;
                    response.Title = "Review Added Successfully";
                    response.Description = "Your review has been successfully submitted.";
                    response.Content = new ReviewResponseDto
                    {
                        ReviewId = newReview.ReviewId,
                        Rating = newReview.Rating ?? 0,
                        ProductId = newReview.ProductId,
                        UserId = newReview.UserId,
                        UserName = newReview.User?.UserName!
                    };
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Adding or Updating Review";
                response.Description = "An error occurred while adding or updating the review.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<string>> DeleteReviewAsync(int reviewId)
        {
            var response = new ResponseModel<string>();

            try
            {
                // Retrieve the review to check if it exists
                var review = await _reviewRepository.GetReviewByIdAsync(reviewId);

                if (review == null!)
                {
                    response.Success = false;
                    response.Title = "Review Not Found";
                    response.Description = $"Review with ID '{reviewId}' does not exist.";
                    return response;
                }

                // Delete the review
                await _reviewRepository.DeleteReviewAsync(review);

                response.Success = true;
                response.Title = "Review Deleted Successfully";
                response.Description = "The review has been successfully deleted.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Deleting Review";
                response.Description = "An error occurred while deleting the review.";
                response.ExceptionMessage = ex.Message;

                // Log the exception
                _logger.LogError(ex, "Error occurred while deleting review with ID: {ReviewId}", reviewId);
            }

            return response;
        }

        public async Task<ResponseModel<Review>> GetReviewByIdAsync(int reviewId)
        {
            var response = new ResponseModel<Review>();

            try
            {
                // Fetch the review by ID
                var review = await _reviewRepository.GetReviewByIdAsync(reviewId);

                if (review == null!)
                {
                    response.Success = false;
                    response.Title = "Review Not Found";
                    response.Description = $"Review with ID '{reviewId}' does not exist.";
                    return response;
                }

                response.Success = true;
                response.Content = review;
                response.Title = "Review Retrieved Successfully";
                response.Description = "The review has been successfully retrieved.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Review";
                response.Description = "An error occurred while retrieving the review.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }
        public async Task<ResponseModel<IList<Review>>> GetReviewsByProductIdAsync(int productId)
        {
            var response = new ResponseModel<IList<Review>>();

            try
            {
                // Retrieve reviews by product ID from the repository
                var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
                int totalCount = reviews.Count;

                // Build the response
                response.Success = true;
                response.Content = reviews;
                response.Title = "Reviews Retrieved Successfully";
                response.Description = $"The reviews for the product have been successfully retrieved. Total Reviews: {totalCount}";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Reviews";
                response.Description = "An error occurred while retrieving the reviews.";
                response.ExceptionMessage = ex.Message;

                // Log the exception (assuming a logger is available)
                _logger.LogError(ex, "Error occurred while retrieving reviews for product ID: {ProductId}", productId);
            }

            return response;
        }

        public async Task<ResponseModel<IList<Review>>> GetReviewsByUserIdAsync(string userId)
        {
            var response = new ResponseModel<IList<Review>>();

            try
            {
                // Retrieve reviews by user ID from the repository
                var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);

                // Build the response
                response.Success = true;
                response.Content = reviews;
                response.Title = "Reviews Retrieved Successfully";
                response.Description = "The reviews by the user have been successfully retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Reviews";
                response.Description = "An error occurred while retrieving the reviews.";
                response.ExceptionMessage = ex.Message;

                // Log the exception
                _logger.LogError(ex, "Error occurred while retrieving reviews for user ID: {UserId}", userId);
            }

            return response;
        }

        public async Task<ResponseModel<string>> GetAverageRatingAsync(int productId)
        {
            var response = new ResponseModel<string>();

            try
            {
                // Check if the product exists
                var productExists = await _reviewRepository.ProductExistsAsync(productId);

                if (!productExists)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{productId}' does not exist.";
                    return response;
                }

                // Fetch reviews for the product
                var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);

                // Calculate and set average rating response
                if (reviews.Count == 0)
                {
                    response.Success = true;
                    response.Content = "No ratings available for this product.";
                    response.Title = "Average Rating";
                    response.Description = "No reviews have been submitted for this product.";
                }
                else
                {
                    var averageRating = reviews.Average(r => r.Rating);
                    response.Success = true;
                    response.Content = averageRating.ToString()!; // Format to one decimal place
                    response.Title = "Average Rating Retrieved Successfully";
                    response.Description = $"The average rating for the product is {averageRating}.";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Average Rating";
                response.Description = "An error occurred while retrieving the average rating.";
                response.ExceptionMessage = ex.Message;

                // Log the exception
                _logger.LogError(ex, "Error occurred while retrieving average rating for product ID: {ProductId}", productId);
            }

            return response;
        }

        public async Task<ResponseModel<string>> GetAverageRatingByShopIdAsync(int shopId)
        {
            var response = new ResponseModel<string>();

            try
            {
                // Retrieve all products associated with the shop
                var products = await _reviewRepository.GetProductsByShopIdAsync(shopId);

                if (products == null! || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for shop with ID '{shopId}'.";
                    return response;
                }

                // Get all reviews for the products in this shop
                var productIds = products.Select(p => p.ProductId).ToList();
                var reviews = await _reviewRepository.GetReviewsByProductIdsAsync(productIds);

                // Check if there are any reviews and calculate the average rating
                if (!reviews.Any())
                {
                    response.Success = true;
                    response.Content = "No ratings available for this shop.";
                    response.Title = "Average Rating";
                    response.Description = "No reviews have been submitted for products in this shop.";
                }
                else
                {
                    var averageRating = reviews.Average(r => r.Rating);
                    response.Success = true;
                    response.Content = averageRating.ToString()!; // Format to one decimal place
                    response.Title = "Average Rating Retrieved Successfully";
                    response.Description = $"The average rating for the shop is {averageRating}.";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Average Rating";
                response.Description = "An error occurred while retrieving the average rating for the shop.";
                response.ExceptionMessage = ex.Message;

                // Log the exception (assuming a logger is available)
                _logger.LogError(ex, "Error occurred while retrieving average rating for shop ID: {ShopId}", shopId);
            }

            return response;
        }

        public async Task<ResponseModel<UserRatingResponseDto>> GetUserRatingAsync(int productId, string userId)
        {
            var response = new ResponseModel<UserRatingResponseDto>();

            try
            {
                // Check if the product exists
                if (!await _reviewRepository.ProductExistsAsync(productId))
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{productId}' does not exist.";
                    return response;
                }

                // Fetch the review
                var review = await _reviewRepository.GetExistingReviewAsync(productId, userId);

                if (review == null!)
                {
                    response.Success = false;
                    response.Title = "Review Not Found";
                    response.Description = "No review found for the specified user and product.";
                }
                else
                {
                    response.Success = true;
                    response.Title = "Review Found";
                    response.Description = "The review for the specified user and product has been retrieved.";
                    response.Content = new UserRatingResponseDto
                    {
                        ProductId = review.ProductId,
                        UserId = review.UserId,
                        Rating = review.Rating
                    };
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Fetching Review";
                response.Description = "An error occurred while fetching the review.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

    }
}
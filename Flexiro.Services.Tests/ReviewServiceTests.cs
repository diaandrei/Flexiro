using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Flexiro.Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly ReviewService _reviewService;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ReviewService>> _loggerMock;
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;

        public ReviewServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ReviewService>>();
            _reviewRepositoryMock = new Mock<IReviewRepository>();

            _reviewService = new ReviewService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _reviewRepositoryMock.Object);
        }

        [Fact]
        public async Task AddOrUpdateReviewAsync_InvalidRating_ReturnsError()
        {
            var dto = new AddReviewRequestDto { ProductId = 1, UserId = "testUser", Rating = 6 };
            var result = await _reviewService.AddOrUpdateReviewAsync(dto);
            Assert.False(result.Success);
            Assert.Equal("Invalid Rating", result.Title);
        }

        [Fact]
        public async Task AddOrUpdateReviewAsync_ProductNotFound_ReturnsError()
        {
            var dto = new AddReviewRequestDto { ProductId = 99, UserId = "testUser", Rating = 5 };
            _reviewRepositoryMock.Setup(repo => repo.ProductExistsAsync(dto.ProductId))
                .ReturnsAsync(false);
            var result = await _reviewService.AddOrUpdateReviewAsync(dto);
            Assert.False(result.Success);
            Assert.Equal("Product Not Found", result.Title);
        }

        [Fact]
        public async Task AddOrUpdateReviewAsync_UpdateExistingReview_Success()
        {
            var dto = new AddReviewRequestDto { ProductId = 1, UserId = "testUser", Rating = 4 };
            var existing = new Review { ReviewId = 100, ProductId = 1, UserId = "testUser", Rating = 3 };
            _reviewRepositoryMock.Setup(repo => repo.ProductExistsAsync(dto.ProductId))
                .ReturnsAsync(true);
            _reviewRepositoryMock.Setup(repo => repo.GetExistingReviewAsync(dto.ProductId, dto.UserId))
                .ReturnsAsync(existing);
            _reviewRepositoryMock.Setup(repo => repo.UpdateReviewAsync(existing))
                .Returns(Task.CompletedTask);

            var result = await _reviewService.AddOrUpdateReviewAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Review Updated Successfully", result.Title);
            Assert.Equal(4, result.Content.Rating);
        }

        [Fact]
        public async Task AddOrUpdateReviewAsync_AddNewReview_Success()
        {
            var dto = new AddReviewRequestDto { ProductId = 1, UserId = "testUser", Rating = 5 };
            _reviewRepositoryMock.Setup(repo => repo.ProductExistsAsync(dto.ProductId))
                .ReturnsAsync(true);
            _reviewRepositoryMock.Setup(repo => repo.GetExistingReviewAsync(dto.ProductId, dto.UserId))
                .ReturnsAsync((Review)null);
            _reviewRepositoryMock.Setup(repo => repo.AddReviewAsync(It.IsAny<Review>()))
                .Returns(Task.CompletedTask);

            var result = await _reviewService.AddOrUpdateReviewAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Review Added Successfully", result.Title);
            Assert.Equal(5, result.Content.Rating);
        }

        [Fact]
        public async Task DeleteReviewAsync_NotFound_ReturnsError()
        {
            var reviewId = 999;
            _reviewRepositoryMock.Setup(repo => repo.GetReviewByIdAsync(reviewId))
                .ReturnsAsync((Review)null);

            var result = await _reviewService.DeleteReviewAsync(reviewId);

            Assert.False(result.Success);
            Assert.Equal("Review Not Found", result.Title);
        }

        [Fact]
        public async Task DeleteReviewAsync_Success()
        {
            var reviewId = 100;
            var existingReview = new Review { ReviewId = 100 };
            _reviewRepositoryMock.Setup(repo => repo.GetReviewByIdAsync(reviewId))
                .ReturnsAsync(existingReview);
            _reviewRepositoryMock.Setup(repo => repo.DeleteReviewAsync(existingReview))
                .Returns(Task.CompletedTask);

            var result = await _reviewService.DeleteReviewAsync(reviewId);

            Assert.True(result.Success);
            Assert.Equal("Review Deleted Successfully", result.Title);
        }

        [Fact]
        public async Task GetReviewByIdAsync_NotFound_ReturnsError()
        {
            var reviewId = 999;
            _reviewRepositoryMock.Setup(repo => repo.GetReviewByIdAsync(reviewId))
                .ReturnsAsync((Review)null);

            var result = await _reviewService.GetReviewByIdAsync(reviewId);

            Assert.False(result.Success);
            Assert.Equal("Review Not Found", result.Title);
        }

        [Fact]
        public async Task GetReviewByIdAsync_Success()
        {
            var reviewId = 100;
            var existingReview = new Review { ReviewId = 100 };
            _reviewRepositoryMock.Setup(repo => repo.GetReviewByIdAsync(reviewId))
                .ReturnsAsync(existingReview);

            var result = await _reviewService.GetReviewByIdAsync(reviewId);

            Assert.True(result.Success);
            Assert.Equal("Review Retrieved Successfully", result.Title);
            Assert.Equal(100, result.Content.ReviewId);
        }

        [Fact]
        public async Task GetReviewsByProductIdAsync_Success()
        {
            var productId = 1;
            var reviews = new List<Review> { new Review { ReviewId = 10 } };
            _reviewRepositoryMock.Setup(repo => repo.GetReviewsByProductIdAsync(productId))
                .ReturnsAsync(reviews);

            var result = await _reviewService.GetReviewsByProductIdAsync(productId);

            Assert.True(result.Success);
            Assert.Equal("Reviews Retrieved Successfully", result.Title);
            Assert.Single(result.Content);
        }

        [Fact]
        public async Task GetReviewsByUserIdAsync_Success()
        {
            var userId = "testUser";
            var reviews = new List<Review> { new Review { ReviewId = 20 } };
            _reviewRepositoryMock.Setup(repo => repo.GetReviewsByUserIdAsync(userId))
                .ReturnsAsync(reviews);

            var result = await _reviewService.GetReviewsByUserIdAsync(userId);

            Assert.True(result.Success);
            Assert.Equal("Reviews Retrieved Successfully", result.Title);
            Assert.Single(result.Content);
        }

        [Fact]
        public async Task GetAverageRatingAsync_ProductNotFound()
        {
            var productId = 999;
            _reviewRepositoryMock.Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(false);

            var result = await _reviewService.GetAverageRatingAsync(productId);

            Assert.False(result.Success);
            Assert.Equal("Product Not Found", result.Title);
        }

        [Fact]
        public async Task GetAverageRatingAsync_NoReviews()
        {
            var productId = 1;
            _reviewRepositoryMock.Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(true);
            _reviewRepositoryMock.Setup(repo => repo.GetReviewsByProductIdAsync(productId))
                .ReturnsAsync(new List<Review>());

            var result = await _reviewService.GetAverageRatingAsync(productId);

            Assert.True(result.Success);
            Assert.Equal("No ratings available for this product.", result.Content);
        }

        [Fact]
        public async Task GetAverageRatingAsync_HasReviews()
        {
            var productId = 1;
            _reviewRepositoryMock.Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(true);
            _reviewRepositoryMock.Setup(repo => repo.GetReviewsByProductIdAsync(productId))
                .ReturnsAsync(new List<Review>
                {
                    new Review { Rating = 4 },
                    new Review { Rating = 5 }
                });

            var result = await _reviewService.GetAverageRatingAsync(productId);

            Assert.True(result.Success);
            Assert.Equal("Average Rating Retrieved Successfully", result.Title);
            Assert.Equal("4.5", result.Content);
        }

        [Fact]
        public async Task GetAverageRatingByShopIdAsync_NoProducts()
        {
            var shopId = 10;
            _reviewRepositoryMock.Setup(repo => repo.GetProductsByShopIdAsync(shopId))
                .ReturnsAsync(new List<Product>());

            var result = await _reviewService.GetAverageRatingByShopIdAsync(shopId);

            Assert.False(result.Success);
            Assert.Equal("No Products Found", result.Title);
        }

        [Fact]
        public async Task GetAverageRatingByShopIdAsync_NoReviews()
        {
            var shopId = 10;
            _reviewRepositoryMock.Setup(repo => repo.GetProductsByShopIdAsync(shopId))
                .ReturnsAsync(new List<Product> { new Product { ProductId = 1 } });
            _reviewRepositoryMock.Setup(repo => repo.GetReviewsByProductIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Review>());

            var result = await _reviewService.GetAverageRatingByShopIdAsync(shopId);

            Assert.True(result.Success);
            Assert.Equal("No ratings available for this shop.", result.Content);
        }

        [Fact]
        public async Task GetAverageRatingByShopIdAsync_HasReviews()
        {
            var shopId = 10;
            _reviewRepositoryMock.Setup(repo => repo.GetProductsByShopIdAsync(shopId))
                .ReturnsAsync(new List<Product> { new Product { ProductId = 1 } });
            _reviewRepositoryMock.Setup(repo => repo.GetReviewsByProductIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Review> { new Review { Rating = 5 }, new Review { Rating = 4 } });

            var result = await _reviewService.GetAverageRatingByShopIdAsync(shopId);

            Assert.True(result.Success);
            Assert.Equal("Average Rating Retrieved Successfully", result.Title);
            Assert.Equal("4.5", result.Content);
        }

        [Fact]
        public async Task GetUserRatingAsync_ProductNotFound_ReturnsError()
        {
            var productId = 999;
            var userId = "testUser";
            _reviewRepositoryMock.Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(false);

            var result = await _reviewService.GetUserRatingAsync(productId, userId);

            Assert.False(result.Success);
            Assert.Equal("Product Not Found", result.Title);
        }

        [Fact]
        public async Task GetUserRatingAsync_Success_ReturnsReview()
        {
            var productId = 1;
            var userId = "testUser";
            var existingReview = new Review { ProductId = 1, UserId = "testUser", Rating = 4 };
            _reviewRepositoryMock.Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(true);
            _reviewRepositoryMock.Setup(repo => repo.GetExistingReviewAsync(productId, userId))
                .ReturnsAsync(existingReview);

            var result = await _reviewService.GetUserRatingAsync(productId, userId);

            Assert.True(result.Success);
            Assert.Equal("Review Found", result.Title);
            Assert.Equal(4, result.Content.Rating);
        }
    }
}

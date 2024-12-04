using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flexiro.Services.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewRepository> _logger;
        public ReviewRepository(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReviewRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<bool> ProductExistsAsync(int productId)
        {
            return await _unitOfWork.Repository
                .GetQueryable<Product>(p => p.ProductId == productId)
                .AnyAsync();
        }

        public async Task<Review> GetExistingReviewAsync(int productId, string userId)
        {
            return (await _unitOfWork.Repository
                .GetQueryable<Review>(r => r.ProductId == productId && r.UserId == userId)
                .FirstOrDefaultAsync())!;
        }

        public async Task AddReviewAsync(Review review)
        {
            await _unitOfWork.Repository.AddAsync(review);
            await _unitOfWork.Repository.CompleteAsync();
        }

        public async Task UpdateReviewAsync(Review review)
        {
            _unitOfWork.Repository.Update(review);
            await _unitOfWork.Repository.CompleteAsync();
        }
        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            return (await _unitOfWork.Repository
                .GetQueryable<Review>(r => r.ReviewId == reviewId)
                .FirstOrDefaultAsync())!;
        }

        public async Task DeleteReviewAsync(Review review)
        {
            _unitOfWork.Repository.HardDelete(review);
            await _unitOfWork.Repository.CompleteAsync();
        }

        public async Task<IList<Review>> GetReviewsByProductIdAsync(int productId)
        {
            return await _unitOfWork.Repository
                .GetQueryable<Review>(r => r.ProductId == productId)
                .ToListAsync();
        }

        public async Task<IList<Review>> GetReviewsByUserIdAsync(string userId)
        {
            return await _unitOfWork.Repository
                .GetQueryable<Review>(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<IList<Product>> GetProductsByShopIdAsync(int shopId)
        {
            return await _unitOfWork.Repository
                .GetQueryable<Product>(p => p.ShopId == shopId)
                .ToListAsync();
        }

        public async Task<IList<Review>> GetReviewsByProductIdsAsync(IList<int> productIds)
        {
            return await _unitOfWork.Repository
                .GetQueryable<Review>(r => productIds.Contains(r.ProductId))
                .ToListAsync();
        }
    }
}
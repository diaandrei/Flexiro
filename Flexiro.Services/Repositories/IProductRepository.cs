using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Repositories
{
    public interface IProductRepository
    {
        Task<Product> CreateProductAsync(ProductCreateDto productDto);
        Task<Product> UpdateProductAsync(int productId, ProductUpdateDto productDto);
        Task<List<string>> GetAllCategoryNamesAsync();
        Task<bool> DeleteProductAsync(int productId);
        Task<List<Product>> GetAllProductsAsync(int shopId);
        Task<List<ProductResponseDto>> GetProductResponsesAsync(List<Product> products);
        Task<ProductDetailResponseDto> GetProductDetailsByIdAsync(int productId, string userId);
        Task<Product> GetProductByIdAsync(int productId);
        Task<IEnumerable<ProductResponse>> GetProductsByShopIdAsync(int shopId, string userId);
        Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<ProductResponseDto>> SearchProductsByNameAsync(string productName);
        Task<UserWishlist> AddProductToWishlistAsync(int productId, string userId, int shopId);
        Task<bool> RemoveProductFromWishlistAsync(int productId, string userId, int shopId);
        Task<Product> ChangeProductStatusAsync(int productId, int newStatus);
        Task UpdateProductImagePaths(int productId, List<string> imagePaths);
        Task<List<ProductSaleResponseDto>> GetSaleProductsAsync();
        Task<List<ProductTopRatedDto>> GetTopRatedAffordableProductsAsync();
    }
}
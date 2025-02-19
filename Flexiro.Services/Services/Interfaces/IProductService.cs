using Flexiro.Application.DTOs;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface IProductService
    {
        Task<ResponseModel<ProductResponseDto>> CreateProductAsync(ProductCreateDto productDto);
        Task<ResponseModel<ProductResponseDto>> UpdateProductAsync(int productId, ProductUpdateDto productDto);
        Task<ResponseModel<object>> RemoveProductFromWishlistAsync(int productId, string userId, int shopId);
        Task<bool> DeleteProductAsync(int productId);
        Task<ResponseModel<ProductListsDto>> GetAllProductsAsync(int shopId);
        Task<ResponseModel<ProductDetailResponseDto>> GetProductDetailsByIdAsync(int productId, string userId);
        Task<ResponseModel<IEnumerable<ProductResponse>>> GetProductsByShopIdAsync(int shopId, string userId);
        Task<ResponseModel<IEnumerable<ProductResponseDto>>> GetProductsByCategoryIdAsync(int categoryId);
        Task<ResponseModel<IEnumerable<ProductResponseDto>>> SearchProductsByNameAsync(string productName);
        Task<ResponseModel<UserWishlistResponseDto>> AddProductToWishlistAsync(int productId, string userId, int shopId);
        Task<ResponseModel<string>> ChangeProductStatusAsync(int productId, int newStatus);
        Task UpdateProductImagePaths(int productId, List<string> imagePaths);
        Task<ResponseModel<List<ProductSaleResponseDto>>> GetSaleProductsAsync();
        Task<List<ProductTopRatedDto>> GetTopRatedAffordableProductsAsync();
        Task<ResponseModel<List<string>>> GetAllCategoryNamesAsync();
        Task<ResponseModel<List<WishlistProductResponseDto>>> GetWishlistProductsByShopAsync(int shopId);
        Task<ResponseModel<object>> AddOrUpdateDiscountPercentageAsync(int productId, UpdateDiscountDto discountDto);
        Task<ResponseModel<List<WishlistProductResponseDto>>> GetWishlistProductsByUserAsync(string userId);
        Task UpdateProductTotalSoldAsync(int productId, int quantitySold);
    }
}
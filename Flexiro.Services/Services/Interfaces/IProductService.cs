using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface IProductService
    {
        Task<ResponseModel<ProductResponseDto>> CreateProductAsync(ProductCreateDto productDto);
        Task<ResponseModel<ProductResponseDto>> UpdateProductAsync(int productId, ProductUpdateDto productDto);
        Task<bool> DeleteProductAsync(int productId);
        Task<ResponseModel<ProductListsDto>> GetAllProductsAsync();
        Task<ResponseModel<ProductDetailResponseDto>> GetProductDetailsByIdAsync(int productId, string baseUrl);
        Task<ResponseModel<IEnumerable<ProductResponse>>> GetProductsByShopIdAsync(int shopId, string baseUrl);
        Task<ResponseModel<IEnumerable<ProductResponseDto>>> GetProductsByCategoryIdAsync(int categoryId);
        Task<ResponseModel<IEnumerable<ProductResponseDto>>> SearchProductsByNameAsync(string productName);
        Task<ResponseModel<UserWishlistResponseDto>> AddProductToWishlistAsync(int productId, string userId, int shopId);
        Task<ResponseModel<string>> ChangeProductStatusAsync(int productId, ProductStatus newStatus);
        Task UpdateProductImagePaths(int productId, List<string> imagePaths);
        Task<ResponseModel<List<ProductSaleResponseDto>>> GetSaleProductsAsync();
        Task<List<ProductTopRatedDto>> GetTopRatedAffordableProductsAsync(string baseUrl);
    }
}
using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Flexiro.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IReviewService _reviewService;
        private readonly IProductRepository _productRepository;
        private readonly IBlobStorageService _blobStorageService;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IReviewService reviewService,
            IProductRepository productRepository,
            IBlobStorageService blobStorageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _reviewService = reviewService;
            _productRepository = productRepository;
            _blobStorageService = blobStorageService;
        }

        public async Task<ResponseModel<ProductResponseDto>> CreateProductAsync(ProductCreateDto productDto)
        {
            var response = new ResponseModel<ProductResponseDto>();

            try
            {
                var product = await _productRepository.CreateProductAsync(productDto);

                var imageUrls = new List<string>();

                foreach (var imageFile in productDto.ProductImages)
                {
                    if (imageFile.Length > 0)
                    {
                        await using var stream = imageFile.OpenReadStream();
                        var imageUrl = await _blobStorageService.UploadImageAsync(stream, imageFile.FileName);
                        imageUrls.Add(imageUrl);
                    }
                }

                await UpdateProductImagePaths(product.ProductId, imageUrls);

                var productResponse = _mapper.Map<ProductResponseDto>(product);

                response.Success = true;
                response.Content = productResponse;
                response.Title = "Product Created Successfully";
                response.Description = $"Product '{product.ProductName}' has been added.";

                Log.Information("Product created successfully: {ProductId}", product.ProductId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Creating Product";
                response.Description = "An error occurred while creating the product.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error creating product");
            }

            return response;
        }

        public async Task<ResponseModel<List<string>>> GetAllCategoryNamesAsync()
        {
            var response = new ResponseModel<List<string>>();

            try
            {
                var categoryNames = await _productRepository.GetAllCategoryNamesAsync();

                if (categoryNames == null! || !categoryNames.Any())
                {
                    response.Success = false;
                    response.Title = "No Categories Found";
                    response.Description = "No categories exist in the database.";
                    return response;
                }

                response.Success = true;
                response.Content = categoryNames;
                response.Title = "Categories Retrieved Successfully";
                response.Description = $"{categoryNames.Count} categories have been retrieved.";

                Log.Information("Categories retrieved successfully");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Categories";
                response.Description = "An error occurred while retrieving categories.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error retrieving categories");
            }

            return response;
        }

        public async Task UpdateProductImagePaths(int productId, List<string> imagePaths)
        {
            var product = await _unitOfWork.Repository
                .GetQueryable<Product>(s => s.ProductId == productId)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new ArgumentException($"Product with ID {productId} not found.");
            }

            if (product.ProductImages == null!)
            {
                product.ProductImages = new List<ProductImage>();
            }

            var currentImagesCount = product.ProductImages.Count;
            var extraImagesCount = currentImagesCount - imagePaths.Count;

            if (extraImagesCount > 0)
            {
                for (int i = 0; i < extraImagesCount; i++)
                {
                    var imageToRemove = product.ProductImages.Last();
                    product.ProductImages.Remove(imageToRemove);
                }
            }

            for (int i = 0; i < imagePaths.Count; i++)
            {
                if (i < product.ProductImages.Count)
                {
                    product.ProductImages.ElementAt(i).Path = imagePaths[i];
                }
                else
                {
                    product.ProductImages.Add(new ProductImage { Path = imagePaths[i] });
                }
            }

            _unitOfWork.Repository.Update(product);
            await _unitOfWork.Repository.CompleteAsync();
        }

        public async Task<ResponseModel<ProductResponseDto>> UpdateProductAsync(int productId, ProductUpdateDto productDto)
        {
            var response = new ResponseModel<ProductResponseDto>();

            try
            {
                var updatedProduct = await _productRepository.UpdateProductAsync(productId, productDto);

                if (updatedProduct == null!)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{productId}' does not exist.";
                    return response;
                }

                var productResponse = _mapper.Map<ProductResponseDto>(updatedProduct);

                response.Success = true;
                response.Content = productResponse;
                response.Title = "Product Updated Successfully";
                response.Description = $"Product '{updatedProduct.ProductName}' has been updated.";

                Log.Information("Product updated successfully: {ProductId}", productId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Updating Product";
                response.Description = "An error occurred while updating the product.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error updating product {ProductId}", productId);
            }

            return response;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                var deletionSuccessful = await _productRepository.DeleteProductAsync(productId);

                if (!deletionSuccessful)
                {
                    Log.Information("No product found to delete: {ProductId}", productId);
                    return false;
                }

                Log.Information("Product deleted successfully: {ProductId}", productId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting product {ProductId}", productId);
                return false;
            }
        }

        public async Task<ResponseModel<ProductListsDto>> GetAllProductsAsync(int shopId)
        {
            var response = new ResponseModel<ProductListsDto>();

            try
            {
                var products = await _productRepository.GetAllProductsAsync(shopId);

                var forSellProducts = products.Where(p => p.Status == ProductStatus.ForSell).ToList();
                var draftProducts = products.Where(p => p.Status == ProductStatus.Draft).ToList();
                var forSaleProducts = products
                    .Where(p => p.Status == ProductStatus.ForSell && p.DiscountPercentage != 0)
                    .ToList();
                var notForSaleProducts = products
                    .Where(p => p.Status == ProductStatus.ForSell && p.Availability == AvailabilityStatus.NotForSale && p.DiscountPercentage == 0)
                    .ToList();
                var soldOutProducts = products.Where(p => p.Status == ProductStatus.SoldOut).ToList();

                // Generate responses for each category
                var forSellProductResponses = await _productRepository.GetProductResponsesAsync(forSellProducts);
                var draftProductResponses = await _productRepository.GetProductResponsesAsync(draftProducts);
                var forSaleProductResponses = await _productRepository.GetProductResponsesAsync(forSaleProducts);
                var notForSaleProductResponses = await _productRepository.GetProductResponsesAsync(notForSaleProducts);
                var soldOutProductResponses = await _productRepository.GetProductResponsesAsync(soldOutProducts);

                response.Success = true;
                response.Content = new ProductListsDto
                {
                    ForSellProducts = forSellProductResponses,
                    DraftProducts = draftProductResponses,
                    ForSaleProducts = forSaleProductResponses,
                    NotForSaleProducts = notForSaleProductResponses,
                    SoldOutProducts = soldOutProductResponses
                };
                response.Title = "Products Retrieved Successfully";
                response.Description = "Products retrieved based on status.";

                Log.Information("All products retrieved for shop {ShopId}", shopId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Products";
                response.Description = "An error occurred while retrieving the products.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error retrieving products for shop {ShopId}", shopId);
            }

            return response;
        }

        public async Task<ResponseModel<ProductDetailResponseDto>> GetProductDetailsByIdAsync(int productId, string userId)
        {
            var response = new ResponseModel<ProductDetailResponseDto>();

            try
            {
                var productDetail = await _productRepository.GetProductDetailsByIdAsync(productId, userId);

                if (productDetail == null!)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{productId}' does not exist.";
                    return response;
                }

                response.Success = true;
                response.Content = productDetail;
                response.Title = "Product Retrieved Successfully";
                response.Description = $"Product '{productDetail.ProductName}' has been retrieved.";

                Log.Information("Product details retrieved for product {ProductId}", productId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Product";
                response.Description = "An error occurred while retrieving the product.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error retrieving product details for {ProductId}", productId);
            }

            return response;
        }

        public async Task<ResponseModel<IEnumerable<ProductResponse>>> GetProductsByShopIdAsync(int shopId, string userId)
        {
            var response = new ResponseModel<IEnumerable<ProductResponse>>();

            try
            {
                var products = await _productRepository.GetProductsByShopIdAsync(shopId, userId);

                if (products == null! || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for shop ID '{shopId}'.";
                    return response;
                }

                response.Success = true;
                response.Content = products;
                response.Title = "Products Retrieved Successfully";
                response.Description = $"Products for shop ID '{shopId}' have been retrieved.";

                Log.Information("Products retrieved for shop {ShopId}", shopId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Products";
                response.Description = "An error occurred while retrieving the products.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error retrieving products for shop {ShopId}", shopId);
            }

            return response;
        }

        public async Task<ResponseModel<IEnumerable<ProductResponseDto>>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var response = new ResponseModel<IEnumerable<ProductResponseDto>>();

            try
            {
                var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);

                if (products == null! || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for category ID '{categoryId}'.";
                    return response;
                }

                response.Success = true;
                response.Content = products;
                response.Title = "Products Retrieved Successfully";
                response.Description = $"Products for category ID '{categoryId}' have been retrieved.";

                Log.Information("Products retrieved for category {CategoryId}", categoryId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Products";
                response.Description = "An error occurred while retrieving the products.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error retrieving products for category {CategoryId}", categoryId);
            }

            return response;
        }

        public async Task<ResponseModel<IEnumerable<ProductResponseDto>>> SearchProductsByNameAsync(string productName)
        {
            var response = new ResponseModel<IEnumerable<ProductResponseDto>>();

            try
            {
                if (string.IsNullOrWhiteSpace(productName))
                {
                    response.Success = false;
                    response.Title = "Invalid Search Term";
                    response.Description = "Product name cannot be null or empty.";
                    return response;
                }

                var products = await _productRepository.SearchProductsByNameAsync(productName);

                if (products == null! || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for the search term '{productName}'.";
                    return response;
                }

                response.Success = true;
                response.Content = products;
                response.Title = "Products Retrieved Successfully";
                response.Description = $"Products matching '{productName}' have been retrieved.";

                Log.Information("Products retrieved for search term '{ProductName}'", productName);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Searching Products";
                response.Description = "An error occurred while searching for products.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error searching products by name '{ProductName}'", productName);
            }

            return response;
        }

        public async Task<ResponseModel<UserWishlistResponseDto>> AddProductToWishlistAsync(int productId, string userId, int shopId)
        {
            var response = new ResponseModel<UserWishlistResponseDto>();

            try
            {
                var wishlistItem = await _productRepository.AddProductToWishlistAsync(productId, userId, shopId);

                if (wishlistItem == null!)
                {
                    response.Success = false;
                    response.Title = "Error Adding to Wishlist";
                    response.Description = "An error occurred while adding the product to your wishlist.";
                    return response;
                }

                var wishlistResponse = _mapper.Map<UserWishlistResponseDto>(wishlistItem);

                response.Success = true;
                response.Content = wishlistResponse;
                response.Title = "Product Added to Wishlist";
                response.Description = "The product has been successfully added to your wishlist.";

                Log.Information("Product {ProductId} added to wishlist for user {UserId}", productId, userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Adding to Wishlist";
                response.Description = "An error occurred while adding the product to your wishlist.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error adding product {ProductId} to wishlist for user {UserId}", productId, userId);
            }

            return response;
        }

        public async Task<ResponseModel<object>> RemoveProductFromWishlistAsync(int productId, string userId, int shopId)
        {
            var response = new ResponseModel<object>();

            try
            {
                var removed = await _productRepository.RemoveProductFromWishlistAsync(productId, userId);

                if (!removed)
                {
                    response.Success = false;
                    response.Title = "Error Removing from Wishlist";
                    response.Description = "An error occurred while removing the product from your wishlist.";
                    return response;
                }

                response.Success = true;
                response.Title = "Product Removed from Wishlist";
                response.Description = "The product has been successfully removed from your wishlist.";

                Log.Information("Product {ProductId} removed from wishlist for user {UserId}", productId, userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Removing from Wishlist";
                response.Description = "An error occurred while removing the product from your wishlist.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error removing product {ProductId} from wishlist for user {UserId}", productId, userId);
            }

            return response;
        }

        public async Task<ResponseModel<string>> ChangeProductStatusAsync(int productId, int newStatus)
        {
            var response = new ResponseModel<string>();

            var updatedProduct = await _productRepository.ChangeProductStatusAsync(productId, newStatus);

            if (updatedProduct == null!)
            {
                response.Success = false;
                response.Title = "Error Updating Product Status";
                response.Description = "An error occurred while updating the product status.";
                Log.Information("No product found to update status: {ProductId}", productId);
                return response;
            }

            response.Success = true;
            response.Title = "Product Status Updated";
            response.Description = $"Product status successfully updated to {newStatus}.";
            Log.Information("Product {ProductId} status updated to {NewStatus}", productId, newStatus);

            return response;
        }

        public async Task<ResponseModel<List<ProductSaleResponseDto>>> GetSaleProductsAsync()
        {
            var response = new ResponseModel<List<ProductSaleResponseDto>>();

            try
            {
                var saleProductDtos = await _productRepository.GetSaleProductsAsync();

                if (saleProductDtos == null! || !saleProductDtos.Any())
                {
                    response.Success = false;
                    response.Title = "No Sale Products Found";
                    response.Description = "No products are currently on sale.";
                    return response;
                }

                response.Success = true;
                response.Content = saleProductDtos;
                response.Title = "Sale Products Retrieved Successfully";
                response.Description = $"{saleProductDtos.Count} sale products have been retrieved.";

                Log.Information("Sale products retrieved successfully");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Sale Products";
                response.Description = "An error occurred while retrieving the sale products.";
                response.ExceptionMessage = ex.Message;

                Log.Error(ex, "Error retrieving sale products");
            }

            return response;
        }

        public async Task<List<ProductTopRatedDto>> GetTopRatedAffordableProductsAsync()
        {
            var topRatedAffordableProducts = await _unitOfWork.Repository.GetQueryable<Product>()
                .Where(p => p.Status == ProductStatus.ForSell)
                .OrderBy(p => p.PricePerItem)
                .ThenByDescending(p => p.Reviews.Average(r => r.Rating))
                .Take(6)
                .Select(p => new ProductTopRatedDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    PricePerItem = p.PricePerItem,
                    ShopImage = p.Shop.ShopLogo
                })
                .ToListAsync();

            Log.Information("Top-rated affordable products retrieved");
            return topRatedAffordableProducts;
        }

        public async Task<ResponseModel<List<WishlistProductResponseDto>>> GetWishlistProductsByUserAsync(string userId)
        {
            var response = new ResponseModel<List<WishlistProductResponseDto>>();

            try
            {
                // Call the repository to fetch wishlist items
                var wishlistItems = await _productRepository.GetWishlistProductsByUserAsync(userId);

                if (wishlistItems == null! || !wishlistItems.Any())
                {
                    response.Success = false;
                    response.Title = "No Wishlist Items Found";
                    response.Description = "No products were found in your wishlist.";
                    return response;
                }

                // Map wishlist items to response DTO
                var wishlistProductDtos = wishlistItems
                    .Select(item => _mapper.Map<WishlistProductResponseDto>(item))
                    .ToList();

                // Set success response
                response.Success = true;
                response.Content = wishlistProductDtos;
                response.Title = "Wishlist Products Retrieved";
                response.Description = "The products in your wishlist have been successfully retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Fetching Wishlist";
                response.Description = "An error occurred while retrieving the wishlist products.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<List<WishlistProductResponseDto>>> GetWishlistProductsByShopAsync(int shopId)
        {
            var response = new ResponseModel<List<WishlistProductResponseDto>>();

            try
            {
                // Call the repository to fetch wishlist items
                var wishlistItems = await _productRepository.GetWishlistProductsByShopAsync(shopId);

                if (wishlistItems == null! || !wishlistItems.Any())
                {
                    response.Success = false;
                    response.Title = "No items found in your wishlist";
                    response.Description = "No products were found in the wishlist for this shop.";
                    return response;
                }

                // Map wishlist items to response DTO
                var wishlistProductDtos = wishlistItems
                    .Select(item => _mapper.Map<WishlistProductResponseDto>(item))
                    .ToList();

                // Set success response
                response.Success = true;
                response.Content = wishlistProductDtos;
                response.Title = "Wishlist Products Retrieved";
                response.Description = "The products in your wishlist have been retrieved successfully.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Fetching Wishlist";
                response.Description = "An error occurred while fetching the wishlist products.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<object>> AddOrUpdateDiscountPercentageAsync(int productId, UpdateDiscountDto discountDto)
        {
            var response = new ResponseModel<object>();

            try
            {
                // Validate the discount percentage
                if (discountDto.DiscountPercentage < 0 || discountDto.DiscountPercentage > 100)
                {
                    response.Success = false;
                    response.Title = "Invalid Discount Percentage";
                    response.Description = "Discount percentage must be between 0 and 100.";
                    return response;
                }

                // Call repository method to update the discount percentage
                var isUpdated = await _productRepository.AddOrUpdateDiscountPercentageAsync(productId, discountDto.DiscountPercentage);

                if (!isUpdated)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = "The product could not be found or updated.";
                    return response;
                }

                response.Success = true;
                response.Content = true;
                response.Title = "Discount Updated Successfully";
                response.Description = $"Discount percentage has been updated to {discountDto.DiscountPercentage}%.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Updating Discount";
                response.Description = "An error occurred while updating the discount percentage.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task UpdateProductTotalSoldAsync(int productId, int quantitySold)
        {
            var product = await _unitOfWork.Repository
                .GetQueryable<Product>(p => p.ProductId == productId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return;
            }

            product.TotalSold = (product.TotalSold ?? 0) + quantitySold;

            _unitOfWork.Repository.Update(product);
            await _unitOfWork.Repository.CompleteAsync();
        }

    }
}

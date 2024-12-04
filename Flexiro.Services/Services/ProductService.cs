using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Flexiro.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IReviewService _reviewService;
        private readonly IProductRepository _productRepository;
        private readonly IBlobStorageService _blobStorageService;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IReviewService reviewService, IProductRepository productRepository, IBlobStorageService blobStorageService)
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
                // Call repository method to create product
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

                // Map to response DTO
                var productResponse = _mapper.Map<ProductResponseDto>(product);

                response.Success = true;
                response.Content = productResponse;
                response.Title = "Product Created Successfully";
                response.Description = $"Product '{product.ProductName}' has been added.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Creating Product";
                response.Description = "An error occurred while creating the product.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<List<string>>> GetAllCategoryNamesAsync()
        {
            var response = new ResponseModel<List<string>>();
            try
            {
                // Fetch category names
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
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Categories";
                response.Description = "An error occurred while retrieving categories.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task UpdateProductImagePaths(int productId, List<string> imagePaths)
        {
            // Fetch the product including its images
            var product = await _unitOfWork.Repository
                .GetQueryable<Product>(s => s.ProductId == productId)
                .Include(p => p.ProductImages) // Ensure ProductImages are loaded
                .FirstOrDefaultAsync();

            // Check if product exists
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {productId} not found.");
            }

            // Initialize ProductImages collection if it's null
            if (product.ProductImages == null!)
            {
                product.ProductImages = new List<ProductImage>();
            }

            // Remove any extra images if there are more than imagePaths
            var currentImagesCount = product.ProductImages.Count;
            var extraImagesCount = currentImagesCount - imagePaths.Count;

            if (extraImagesCount > 0)
            {
                // Remove excess images from the product
                for (int i = 0; i < extraImagesCount; i++)
                {
                    var imageToRemove = product.ProductImages.Last();
                    product.ProductImages.Remove(imageToRemove); // Remove the image
                }
            }

            // Update or add the image paths
            for (int i = 0; i < imagePaths.Count; i++)
            {
                if (i < product.ProductImages.Count)
                {
                    // Update existing image path
                    product.ProductImages.ElementAt(i).Path = imagePaths[i];
                }
                else
                {
                    // Add new image if there are more paths than images
                    product.ProductImages.Add(new ProductImage { Path = imagePaths[i] });
                }
            }

            // Save changes to the database
            _unitOfWork.Repository.Update(product);
            await _unitOfWork.Repository.CompleteAsync();
        }

        public async Task<ResponseModel<ProductResponseDto>> UpdateProductAsync(int productId, ProductUpdateDto productDto)
        {
            var response = new ResponseModel<ProductResponseDto>();

            try
            {
                // Call repository function to update product
                var updatedProduct = await _productRepository.UpdateProductAsync(productId, productDto);

                if (updatedProduct == null!)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{productId}' does not exist.";
                    return response;
                }

                // Map the updated product to response DTO
                var productResponse = _mapper.Map<ProductResponseDto>(updatedProduct);

                // Set successful response
                response.Success = true;
                response.Content = productResponse;
                response.Title = "Product Updated Successfully";
                response.Description = $"Product '{updatedProduct.ProductName}' has been updated.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Updating Product";
                response.Description = "An error occurred while updating the product.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                // Call repository function to delete product
                var deletionSuccessful = await _productRepository.DeleteProductAsync(productId);

                if (!deletionSuccessful)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<ResponseModel<ProductListsDto>> GetAllProductsAsync(int shopId)
        {
            var response = new ResponseModel<ProductListsDto>();

            try
            {
                // Retrieve products from the repository
                var products = await _productRepository.GetAllProductsAsync(shopId);

                // Categorize products by status and availability
                var forSellProducts = products.Where(p => p.Status == ProductStatus.ForSell).ToList();
                var draftProducts = products.Where(p => p.Status == ProductStatus.Draft).ToList();

                var forSaleProducts = products
                    .Where(p => p.Status == ProductStatus.ForSell && p.Availability == AvailabilityStatus.ForSale && p.DiscountPercentage != 0)
                    .ToList();

                var notForSaleProducts = products
                    .Where(p => p.Status == ProductStatus.ForSell && p.Availability == AvailabilityStatus.NotForSale && p.DiscountPercentage == 0)
                    .ToList();

                // Generate responses for each category
                var forSellProductResponses = await _productRepository.GetProductResponsesAsync(forSellProducts);
                var draftProductResponses = await _productRepository.GetProductResponsesAsync(draftProducts);
                var forSaleProductResponses = await _productRepository.GetProductResponsesAsync(forSaleProducts);
                var notForSaleProductResponses = await _productRepository.GetProductResponsesAsync(notForSaleProducts);

                // Set response content
                response.Success = true;

                response.Content = new ProductListsDto
                {
                    ForSellProducts = forSellProductResponses,
                    DraftProducts = draftProductResponses,
                    ForSaleProducts = forSaleProductResponses,
                    NotForSaleProducts = notForSaleProductResponses
                };
                response.Title = "Products Retrieved Successfully";
                response.Description = "Products retrieved based on status.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Products";
                response.Description = "An error occurred while retrieving the products.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<ProductDetailResponseDto>> GetProductDetailsByIdAsync(int productId, string userId)
        {
            var response = new ResponseModel<ProductDetailResponseDto>();

            try
            {
                // Retrieve the product details from the repository
                var productDetail = await _productRepository.GetProductDetailsByIdAsync(productId, userId);

                // Check if the product details were found
                if (productDetail == null!)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{productId}' does not exist.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = productDetail;
                response.Title = "Product Retrieved Successfully";
                response.Description = $"Product '{productDetail.ProductName}' has been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Product";
                response.Description = "An error occurred while retrieving the product.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IEnumerable<ProductResponse>>> GetProductsByShopIdAsync(int shopId, string userId)
        {
            var response = new ResponseModel<IEnumerable<ProductResponse>>();

            try
            {
                // Retrieve the products from the repository by shop ID
                var products = await _productRepository.GetProductsByShopIdAsync(shopId, userId);

                // Check if any products were found
                if (products == null! || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for shop ID '{shopId}'.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = products;
                response.Title = "Products Retrieved Successfully";
                response.Description = $"Products for shop ID '{shopId}' have been retrieved.";
            }

            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Products";
                response.Description = "An error occurred while retrieving the products.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IEnumerable<ProductResponseDto>>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var response = new ResponseModel<IEnumerable<ProductResponseDto>>();

            try
            {
                // Retrieve the products by category ID
                var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);

                // Check if any products were found
                if (products == null! || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for category ID '{categoryId}'.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = products;
                response.Title = "Products Retrieved Successfully";
                response.Description = $"Products for category ID '{categoryId}' have been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Products";
                response.Description = "An error occurred while retrieving the products.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IEnumerable<ProductResponseDto>>> SearchProductsByNameAsync(string productName)
        {
            var response = new ResponseModel<IEnumerable<ProductResponseDto>>();

            try
            {
                // Ensure productName is not null or empty
                if (string.IsNullOrWhiteSpace(productName))
                {
                    response.Success = false;
                    response.Title = "Invalid Search Term";
                    response.Description = "Product name cannot be null or empty.";
                    return response;
                }

                // Call repository to search products by name
                var products = await _productRepository.SearchProductsByNameAsync(productName);

                // Check if any products were found
                if (products == null! || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for the search term '{productName}'.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = products;
                response.Title = "Products Retrieved Successfully";
                response.Description = $"Products matching '{productName}' have been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Searching Products";
                response.Description = "An error occurred while searching for products.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }


        public async Task<ResponseModel<UserWishlistResponseDto>> AddProductToWishlistAsync(int productId, string userId, int shopId)
        {
            var response = new ResponseModel<UserWishlistResponseDto>();

            try
            {
                // Call the repository method to add the product to the wishlist
                var wishlistItem = await _productRepository.AddProductToWishlistAsync(productId, userId, shopId);

                if (wishlistItem == null!)
                {
                    response.Success = false;
                    response.Title = "Error Adding to Wishlist";
                    response.Description = "An error occurred while adding the product to your wishlist.";
                    return response;
                }

                // Map to response DTO
                var wishlistResponse = _mapper.Map<UserWishlistResponseDto>(wishlistItem);

                // Set successful response
                response.Success = true;
                response.Content = wishlistResponse;
                response.Title = "Product Added to Wishlist";
                response.Description = "The product has been successfully added to your wishlist.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Adding to Wishlist";
                response.Description = "An error occurred while adding the product to your wishlist.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<object>> RemoveProductFromWishlistAsync(int productId, string userId, int shopId)
        {
            var response = new ResponseModel<object>();

            try
            {
                // Call the repository method to remove the product from the wishlist
                var removed = await _productRepository.RemoveProductFromWishlistAsync(productId, userId, shopId);

                if (!removed)
                {
                    response.Success = false;
                    response.Title = "Error Removing from Wishlist";
                    response.Description = "An error occurred while removing the product from your wishlist.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Title = "Product Removed from Wishlist";
                response.Description = "The product has been successfully removed from your wishlist.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Removing from Wishlist";
                response.Description = "An error occurred while removing the product from your wishlist.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<string>> ChangeProductStatusAsync(int productId, int newStatus)
        {
            var response = new ResponseModel<string>();

            // Call the repository function to change the product status
            var updatedProduct = await _productRepository.ChangeProductStatusAsync(productId, newStatus);

            if (updatedProduct == null!)
            {
                response.Success = false;
                response.Title = "Error Updating Product Status";
                response.Description = "An error occurred while updating the product status.";
                return response;
            }

            // Set successful response
            response.Success = true;
            response.Title = "Product Status Updated";
            response.Description = $"Product status successfully updated to {newStatus}.";
            return response;
        }

        public async Task<ResponseModel<List<ProductSaleResponseDto>>> GetSaleProductsAsync()
        {
            var response = new ResponseModel<List<ProductSaleResponseDto>>();

            try
            {
                // Retrieve the sale products from the repository
                var saleProductDtos = await _productRepository.GetSaleProductsAsync();

                // Check if any sale products were found
                if (saleProductDtos == null! || !saleProductDtos.Any())
                {
                    response.Success = false;
                    response.Title = "No Sale Products Found";
                    response.Description = "No products are currently on sale.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = saleProductDtos;
                response.Title = "Sale Products Retrieved Successfully";
                response.Description = $"{saleProductDtos.Count} sale products have been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Sale Products";
                response.Description = "An error occurred while retrieving the sale products.";
                response.ExceptionMessage = ex.Message;
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

            return topRatedAffordableProducts;
        }
    }
}
using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Microsoft.EntityFrameworkCore;
using Flexiro.Services.Services.Interfaces;

namespace Flexiro.Services.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IReviewService _reviewService;
        private readonly IBlobStorageService _blobStorageService;

        public ProductRepository(IUnitOfWork unitOfWork, IMapper mapper, IReviewService reviewService, IBlobStorageService blobStorageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _reviewService = reviewService;
            _blobStorageService = blobStorageService;
        }
        public async Task<List<string>> GetAllCategoryNamesAsync()
        {
            return await _unitOfWork.Repository
                .GetQueryable<Category>()
                .Select(c => c.Name)
                .ToListAsync();
        }

        public async Task<Product> CreateProductAsync(ProductCreateDto productDto)
        {
            // Map DTO to Product entity
            var product = _mapper.Map<Product>(productDto);
            product.DiscountPercentage = 0;

            // Add product to the repository
            await _unitOfWork.Repository.AddAsync(product);
            await _unitOfWork.Repository.CompleteAsync();

            // Create and add product images
            var productImages = new List<ProductImage>();

            foreach (var imageFile in productDto.ProductImages)
            {
                if (imageFile.Length > 0)
                {
                    var productImage = new ProductImage
                    {
                        ProductId = product.ProductId,
                        Path = "",
                        CreatedAt = DateTime.UtcNow,
                    };

                    productImages.Add(productImage);
                }
            }

            // Set product images and update the product
            product.ProductImages = productImages;

            _unitOfWork.Repository.Update(product);
            await _unitOfWork.Repository.CompleteAsync();

            return product;
        }

        public async Task UpdateProductImagePaths(int productId, List<string> imagePaths)
        {
            var product = await _unitOfWork.Repository
                   .GetQueryable<Product>(s => s.ProductId == productId)
                   .FirstOrDefaultAsync();

            if (product != null)
            {
                // Update each product image path
                for (int i = 0; i < imagePaths.Count; i++)
                {
                    product.ProductImages.ElementAt(i).Path = imagePaths[i];
                }

                _unitOfWork.Repository.Update(product);
                await _unitOfWork.Repository.CompleteAsync();
            }
        }

        public async Task<Product> UpdateProductAsync(int productId, ProductUpdateDto productDto)
        {
            // Retrieve the existing product from the repository
            var product = await _unitOfWork.Repository
                .GetQueryable<Product>(p => p.ProductId == productId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return null!;
            }

            // Map updated fields from the DTO to the existing product
            _mapper.Map(productDto, product);

            if (productDto.ProductImages?.Any() == true)
            {
                var productImages = new List<ProductImage>();

                foreach (var imageFile in productDto.ProductImages)
                {
                    if (imageFile.Length > 0)
                    {
                        // Upload image to Blob Storage
                        await using (var stream = imageFile.OpenReadStream())
                        {
                            var imageUrl = await _blobStorageService.UploadImageAsync(stream, imageFile.FileName);  // Get the full image URL from Blob Storage

                            var productImage = new ProductImage
                            {
                                ProductId = product.ProductId,
                                Path = imageUrl,  // Save the full image URL
                                CreatedAt = DateTime.UtcNow,
                            };

                            productImages.Add(productImage);
                        }
                    }
                }

                // Assign the list of uploaded images to the product
                product.ProductImages = productImages;
            }

            // Update the product in the repository
            _unitOfWork.Repository.Update(product);
            await _unitOfWork.Repository.CompleteAsync();

            return product;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                // Retrieve the existing product from the repository
                var product = await _unitOfWork.Repository
                    .GetQueryable<Product>(p => p.ProductId == productId)
                    .FirstOrDefaultAsync();

                // If product does not exist, return false
                if (product == null)
                {
                    return false;
                }

                // Delete the product from the repository
                _unitOfWork.Repository.HardDelete(product);
                await _unitOfWork.Repository.CompleteAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Product>> GetAllProductsAsync(int shopId)
        {
            return await _unitOfWork.Repository
                .GetQueryable<Product>(p => p.ShopId == shopId)
                .Include(p => p.ProductImages)
                .ToListAsync();
        }

        public async Task<List<ProductResponseDto>> GetProductResponsesAsync(List<Product> products)
        {
            var productResponses = new List<ProductResponseDto>();

            foreach (var product in products)
            {
                // Get the average rating and categorize it
                var averageRatingResponse = await _reviewService.GetAverageRatingAsync(product.ProductId);
                string productScore = "Average";

                if (averageRatingResponse.Success)
                {
                    // Check if the response content is a valid number
                    if (double.TryParse(averageRatingResponse.Content, out double averageRating))
                    {
                        // Categorize the product score based on the rating
                        if (averageRating >= 4.5)
                        {
                            productScore = "Good";
                        }
                        else if (averageRating <= 2.5)
                        {
                            productScore = "Bad";
                        }
                    }
                    else if (averageRatingResponse.Content == "No ratings available for this product.")
                    {
                        productScore = "No Ratings";
                    }
                }

                productResponses.Add(new ProductResponseDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    PricePerItem = product.PricePerItem,
                    ShopId = product.ShopId,
                    CategoryId = product.CategoryId,
                    MainImage = product.ProductImages?.FirstOrDefault() != null
                    ? product.ProductImages.First().Path : string.Empty,
                    Weight = product.Weight,
                    ProductCondition = product.ProductCondition,
                    ImportedItem = product.ImportedItem,
                    StockQuantity = product.StockQuantity,
                    SKU = product.SKU,
                    Status = product.Status,
                    Tags = product.Tags.ToList(),
                    ProductScore = productScore  // Set product score based on the average rating
                });
            }

            return productResponses;
        }
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            try
            {
                // Fetch the product by ID, including any related entities
                var product = await _unitOfWork.Repository.GetQueryable<Product>(p => p.ProductId == productId)
                    .Include(p => p.ProductImages) // Include related entities if needed
                    .FirstOrDefaultAsync();

                return product!;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ProductDetailResponseDto> GetProductDetailsByIdAsync(int productId, string userId)
        {
            var product = await _unitOfWork.Repository
                .GetQueryable<Product>(p => p.ProductId == productId && p.Status == ProductStatus.ForSell)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .Include(p => p.Category)
                .FirstOrDefaultAsync();

            // Return null if product does not exist
            if (product == null)
            {
                return null!;
            }

            // Calculate derived values
            var averageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating ?? 0) : 0;

            var totalReviews = product.Reviews.Count;

            var finalPrice = product.DiscountPercentage.HasValue
                ? product.PricePerItem - (product.PricePerItem * (product.DiscountPercentage.Value / 100))
                : product.PricePerItem;

            var isInWishlist = await _unitOfWork.Repository
               .GetQueryable<UserWishlist>(w => w.ProductId == productId && w.UserId == userId)
               .AnyAsync();

            // Map the retrieved product to the ProductDetailResponseDto
            var productDetail = new ProductDetailResponseDto
            {
                ProductId = product.ProductId,
                ShopId = product.ShopId,
                ProductName = product.ProductName,
                Description = product.Description!,
                PricePerItem = finalPrice,
                MainImage = product.ProductImages?.FirstOrDefault() != null
                    ? product.ProductImages.First().Path
                    : string.Empty,
                ImageUrls = product.ProductImages?.Select(img => img.Path).ToList() ?? new List<string>(),
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                IsInWishlist = isInWishlist,
                TotalSold = product.totalsold ?? 0,
                CategoryName = product.Category?.Name!,
                Reviews = product.Reviews.Select(review => new ReviewResponseDto
                {
                    ReviewId = review.ReviewId,
                    ProductId = product.ProductId,
                    UserName = review.User.UserName ?? "Anonymous",
                    Rating = review.Rating ?? 0,
                    Comment = review.Comment ?? "No comment provided."
                }).ToList()
            };

            return productDetail;
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsByShopIdAsync(int shopId, string userId)
        {
            var products = await _unitOfWork.Repository
                .GetQueryable<Product>(p => p.ShopId == shopId && p.Status == ProductStatus.ForSell)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .Include(p => p.Category)
                .ToListAsync();

            // Check if any products were found
            if (products == null! || !products.Any())
            {
                return null!;
            }

            // Map the retrieved products to ProductResponseDTO
            var productResponses = new List<ProductResponse>();

            foreach (var product in products)
            {
                // Check if the product is in the wishlist for the given user
                var isInWishlist = await _unitOfWork.Repository
                    .GetQueryable<UserWishlist>(w => w.ProductId == product.ProductId && w.UserId == userId)
                    .AnyAsync();

                // Calculate discounted or final price
                var finalPrice = product.DiscountPercentage.HasValue && product.DiscountPercentage.Value != 0
                    ? product.PricePerItem - (product.PricePerItem * (product.DiscountPercentage.Value / 100))
                    : product.PricePerItem;

                // Map the product to ProductResponse
                productResponses.Add(new ProductResponse
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description!,
                    PricePerItem = product.PricePerItem,
                    DiscountedPrice = finalPrice,
                    FinalPrice = finalPrice,
                    MainImage = product.ProductImages?.FirstOrDefault() != null
                        ? product.ProductImages.First().Path : string.Empty,
                    ImageUrls = product.ProductImages?.Select(img => img.Path).ToList() ?? new List<string>(),
                    TotalSold = product.totalsold ?? 0,
                    AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating ?? 0) : 0,
                    TotalReviews = product.Reviews.Count,
                    CategoryName = product.Category?.Name!,
                    ShopId = product.ShopId,
                    IsInWishlist = isInWishlist // Check if product is in wishlist
                });
            }

            return productResponses;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            // Retrieve the products from the database by category ID
            var products = await _unitOfWork.Repository
                .GetQueryable<Product>(s => s.CategoryId == categoryId)
                .ToListAsync();

            // Check if any products were found
            if (products == null! || !products.Any())
            {
                return null!;
            }

            // Map the retrieved products to ProductResponseDto
            var productResponses = _mapper.Map<IEnumerable<ProductResponseDto>>(products);

            return productResponses;
        }

        public async Task<IEnumerable<ProductResponseDto>> SearchProductsByNameAsync(string productName)
        {
            // Ensure productName is not null or empty
            if (string.IsNullOrWhiteSpace(productName))
            {
                return null!;
            }

            // Retrieve the products by name (case-insensitive) and include product images
            var products = await _unitOfWork.Repository
                .GetQueryable<Product>(s => s.ProductName.Contains(productName))
                .Include(p => p.ProductImages)
                .ToListAsync();

            // Check if any products were found
            if (products == null! || !products.Any())
            {
                return null!;
            }

            // Map the retrieved products to ProductResponseDto
            var productResponses = _mapper.Map<IEnumerable<ProductResponseDto>>(products);

            return productResponses;
        }


        public async Task<UserWishlist> AddProductToWishlistAsync(int productId, string userId, int shopId)
        {
            try
            {
                // Check if the product exists
                var product = await _unitOfWork.Repository
                    .GetQueryable<Product>(p => p.ProductId == productId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return null!; // Return null if product is not found
                }

                // Check if the wishlist item already exists for this user and product
                var existingWishlistItem = await _unitOfWork.Repository
                    .GetQueryable<UserWishlist>(w => w.ProductId == productId && w.UserId == userId && w.ShopId == shopId)
                    .FirstOrDefaultAsync();

                if (existingWishlistItem != null)
                {
                    return null!; // Return null if the item already exists in the wishlist
                }

                // Create new wishlist item
                var wishlistItem = new UserWishlist
                {
                    ProductId = productId,
                    UserId = userId,
                    ShopId = shopId,
                    CreatedAt = DateTime.UtcNow
                };

                // Add the new wishlist item to the repository
                await _unitOfWork.Repository.AddAsync(wishlistItem);
                await _unitOfWork.Repository.CompleteAsync();

                return wishlistItem; // Return the created wishlist item
            }
            catch (Exception)
            {
                return null!; // Return null if any errors occur
            }
        }

        public async Task<bool> RemoveProductFromWishlistAsync(int productId, string userId, int shopId)
        {
            try
            {
                // Find the wishlist item based on productId, userId, and shopId
                var wishlistItem = await _unitOfWork.Repository
                    .GetQueryable<UserWishlist>(w => w.ProductId == productId && w.UserId == userId && w.ShopId == shopId)
                    .FirstOrDefaultAsync();

                if (wishlistItem == null)
                {
                    return false;
                }

                // Remove the wishlist item from the repository
                _unitOfWork.Repository.HardDelete(wishlistItem);
                await _unitOfWork.Repository.CompleteAsync();

                return true;
            }
            catch (Exception)
            {
                return false; // Return false if any errors occur
            }
        }

        public async Task<Product> ChangeProductStatusAsync(int productId, int newStatus)
        {
            try
            {
                // Retrieve the product
                var product = await _unitOfWork.Repository.GetQueryable<Product>(p => p.ProductId == productId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return null!; // Return null if the product is not found
                }

                // Update product status
                product.Status = (ProductStatus)newStatus;

                // Save changes
                await _unitOfWork.Repository.UpdateAsync(product);
                await _unitOfWork.Repository.CompleteAsync();

                return product; // Return the updated product
            }
            catch (Exception)
            {
                return null!; // Return null if an exception occurs
            }
        }

        public async Task<List<ProductSaleResponseDto>> GetSaleProductsAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow;

                var saleProducts = await _unitOfWork.Repository
                    .GetQueryable<Product>(p => p.DiscountPercentage.HasValue &&
                                                 p.SaleStartDate <= currentDate &&
                                                 p.SaleEndDate >= currentDate)
                    .Include(p => p.ProductImages)
                    .ToListAsync();

                // Map the retrieved products to ProductSaleResponseDto
                var saleProductDtos = saleProducts.Select(p => new ProductSaleResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    MainImage = p.ProductImages.FirstOrDefault()?.Path ?? string.Empty,
                    OriginalPrice = p.PricePerItem,
                    DiscountedPrice = p.PricePerItem - (p.PricePerItem * (p.DiscountPercentage!.Value / 100)),
                    DiscountPercentage = p.DiscountPercentage.Value,
                    SaleEndDate = p.SaleEndDate!.Value,
                    StockQuantity = p.StockQuantity,
                    TotalSold = p.totalsold ?? 0
                }).ToList();

                return saleProductDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving sale products.", ex);
            }
        }

        public async Task<List<ProductTopRatedDto>> GetTopRatedAffordableProductsAsync()
        {
            try
            {
                // Retrieve the top-rated and affordable products 
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
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving top-rated affordable products.", ex);
            }
        }
    }
}
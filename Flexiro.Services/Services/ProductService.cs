using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Flexiro.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel<ProductResponseDto>> CreateProductAsync(ProductCreateDto productDto)
        {
            var response = new ResponseModel<ProductResponseDto>();

            try
            {
                // Map DTO to Product entity
                var product = _mapper.Map<Product>(productDto);

                // Add product to the repository
                await _unitOfWork.Repository.AddAsync(product);
                await _unitOfWork.Repository.CompleteAsync();

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
                product.ProductImages = productImages;
                _unitOfWork.Repository.Update(product);
                await _unitOfWork.Repository.CompleteAsync();
                var productResponse = _mapper.Map<ProductResponseDto>(product);

                // Set successful response
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

        public async Task UpdateProductImagePaths(int productId, List<string> imagePaths)
        {
            var product = await _unitOfWork.Repository
                .GetQueryable<Product>(s => s.ProductId == productId)
                .FirstOrDefaultAsync();

            if (product != null)
            {
                // Update each product image path
                for (var i = 0; i < imagePaths.Count; i++)
                {
                    product.ProductImages.ElementAt(i).Path = imagePaths[i];
                }

                _unitOfWork.Repository.Update(product);
                await _unitOfWork.Repository.CompleteAsync();
            }
        }

        public async Task<ResponseModel<ProductResponseDto>> UpdateProductAsync(int productId,
            ProductUpdateDto productDto)
        {
            var response = new ResponseModel<ProductResponseDto>();

            try
            {
                // Retrieve the existing product from the repository
                var product = await _unitOfWork.Repository
                    .GetQueryable<Product>(s => s.ProductId == productId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{productId}' does not exist.";
                    return response;
                }

                // Map the updated fields from the DTO to the existing product
                _mapper.Map(productDto, product);

                // Update the product in the repository
                _unitOfWork.Repository.Update(product);
                await _unitOfWork.Repository.CompleteAsync();

                var productImages = new List<ProductImage>();

                foreach (var imageFile in productDto.ProductImages!)
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

                product.ProductImages = productImages;
                _unitOfWork.Repository.Update(product);
                await _unitOfWork.Repository.CompleteAsync();

                // Map the updated product to response DTO
                var productResponse = _mapper.Map<ProductResponseDto>(product);

                // Set successful response
                response.Success = true;
                response.Content = productResponse;
                response.Title = "Product Updated Successfully";
                response.Description = $"Product '{product.ProductName}' has been updated.";
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
                // Retrieve the existing product from the repository
                var product = await _unitOfWork.Repository
                    .GetQueryable<Product>(s => s.ProductId == productId)
                    .FirstOrDefaultAsync();

                // Check if product exists
                if (product is null)
                {
                    return false;
                }

                // Delete the product from the repository
                _unitOfWork.Repository.HardDelete(product);
                await _unitOfWork.Repository.CompleteAsync();

                return true; // Product successfully deleted
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ResponseModel<ProductListsDto>> GetAllProductsAsync()
        {
            var response = new ResponseModel<ProductListsDto>();

            try
            {
                // Retrieve all products from the repository
                var products = await _unitOfWork.Repository
                    .GetQueryable<Product>()
                    .ToListAsync();

                var allProducts = products;
                var forSellProducts = products.Where(p => p.Status == ProductStatus.ForSell).ToList();
                var draftProducts = products.Where(p => p.Status == ProductStatus.Draft).ToList();
                var ForSaleProducts = products
                    .Where(p => p.Status == ProductStatus.ForSell && p.Availability == AvailabilityStatus.ForSale)
                    .ToList();

                var notForSaleProducts = products
                    .Where(p => p.Status == ProductStatus.ForSell && p.Availability == AvailabilityStatus.NotForSale)
                    .ToList();

                var forSellProductResponses = forSellProducts.Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    PricePerItem = p.PricePerItem,
                    ShopId = p.ShopId,
                    CategoryId = p.CategoryId,
                    Weight = p.Weight,
                    ProductCondition = p.ProductCondition,
                    ImportedItem = p.ImportedItem,
                    StockQuantity = p.StockQuantity,
                    SKU = p.SKU,
                    Status = p.Status,
                    Tags = p.Tags.ToList()
                }).ToList();

                var draftProductResponses = draftProducts.Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    PricePerItem = p.PricePerItem,
                    ShopId = p.ShopId,
                    CategoryId = p.CategoryId,
                    Weight = p.Weight,
                    ProductCondition = p.ProductCondition,
                    ImportedItem = p.ImportedItem,
                    StockQuantity = p.StockQuantity,
                    SKU = p.SKU,
                    Status = p.Status,
                    Tags = p.Tags.ToList()
                }).ToList();

                var forSaleProductResponses = ForSaleProducts.Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    PricePerItem = p.PricePerItem,
                    ShopId = p.ShopId,
                    CategoryId = p.CategoryId,
                    Weight = p.Weight,
                    ProductCondition = p.ProductCondition,
                    ImportedItem = p.ImportedItem,
                    StockQuantity = p.StockQuantity,
                    SKU = p.SKU,
                    Status = p.Status,
                    Tags = p.Tags.ToList()
                }).ToList();

                var notforSaleProductResponses = notForSaleProducts.Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    PricePerItem = p.PricePerItem,
                    ShopId = p.ShopId,
                    CategoryId = p.CategoryId,
                    Weight = p.Weight,
                    ProductCondition = p.ProductCondition,
                    ImportedItem = p.ImportedItem,
                    StockQuantity = p.StockQuantity,
                    SKU = p.SKU,
                    Status = p.Status,
                    Tags = p.Tags.ToList()
                }).ToList();

                // Assign lists to the response model
                response.Success = true;
                response.Content = new ProductListsDto
                {
                    ForSellProducts = forSellProductResponses,
                    DraftProducts = draftProductResponses,
                    ForSaleProducts = forSaleProductResponses,
                    NotForSellProducts = notforSaleProductResponses
                };
                response.Title = "Products Retrieved Successfully";
                response.Description = "Products retrieved based on status.";
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

        public async Task<ResponseModel<ProductDetailResponseDto>> GetProductDetailsByIdAsync(int productId,
            string baseUrl)
        {
            var response = new ResponseModel<ProductDetailResponseDto>();

            try
            {
                // Retrieve the product from the repository by ID
                var product = await _unitOfWork.Repository
                    .GetQueryable<Product>(p => p.ProductId == productId && p.Status == ProductStatus.ForSell)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync();

                // Check if the product exists
                if (product is null)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = $"Product with ID '{productId}' does not exist.";
                    return response;
                }

                var averageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating ?? 0) : 0;
                var totalReviews = product.Reviews.Count;
                var finalPrice = product.DiscountPercentage.HasValue
                    ? product.PricePerItem - (product.PricePerItem * (product.DiscountPercentage.Value / 100))
                    : product.PricePerItem;

                // Map the retrieved product to response DTO
                var productDetail = new ProductDetailResponseDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description!,
                    PricePerItem = finalPrice,
                    MainImage = product.ProductImages?.FirstOrDefault() != null
                        ? $"{baseUrl}{product.ProductImages.First().Path}"
                        : string.Empty,
                    ImageUrls = product.ProductImages?.Select(img => $"{baseUrl}{img.Path}").ToList() ??
                                new List<string>(),
                    AverageRating = averageRating,
                    TotalReviews = totalReviews,
                    TotalSold = product.totalsold ?? 0,
                    CategoryName = product.Category?.Name!,
                    Reviews = product.Reviews.Select(review => new ReviewResponseDto
                    {
                        UserName = review.User.UserName ?? "Anonymous",
                        Rating = review.Rating ?? 0,
                        Comment = review.Comment ?? "No comment provided."
                    }).ToList()
                };

                // Set successful response
                response.Success = true;
                response.Content = productDetail;
                response.Title = "Product Retrieved Successfully";
                response.Description = $"Product '{product.ProductName}' has been retrieved.";
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

        public async Task<ResponseModel<IEnumerable<ProductResponse>>> GetProductsByShopIdAsync(int shopId,
            string baseUrl)
        {
            var response = new ResponseModel<IEnumerable<ProductResponse>>();
            try
            {
                // Retrieve the products from the repository by shop ID
                var products = await _unitOfWork.Repository
                    .GetQueryable<Product>(p => p.ShopId == shopId && p.Status == ProductStatus.ForSell)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Reviews)
                    .Include(p => p.Category)
                    .ToListAsync();

                // Check if any products were found
                if (products?.Any() != true)
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for shop ID '{shopId}'.";
                    return response;
                }

                var productResponses = products.Select(product => new ProductResponse
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description!,
                    PricePerItem = product.PricePerItem,
                    DiscountedPrice = (product.DiscountPercentage != 0
                        ? product.PricePerItem - (product.PricePerItem * (product.DiscountPercentage!.Value / 100))
                        : 0),
                    FinalPrice = product.DiscountPercentage != 0
                        ? (decimal)(product.PricePerItem -
                                    (product.PricePerItem * (product.DiscountPercentage.Value / 100)))
                        : product.PricePerItem,
                    MainImage = product.ProductImages?.FirstOrDefault() != null
                        ? $"{baseUrl}{product.ProductImages.First().Path}"
                        : string.Empty,
                    ImageUrls = product.ProductImages?.Select(img => $"{baseUrl}{img.Path}").ToList() ??
                                new List<string>(),
                    TotalSold = product.totalsold ?? 0,
                    AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating ?? 0) : 0,
                    TotalReviews = product.Reviews.Count,
                    CategoryName = product.Category?.Name!
                }).ToList();

                // Set successful response
                response.Success = true;
                response.Content = productResponses;
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
                var products = await _unitOfWork.Repository
                    .GetQueryable<Product>(s => s.CategoryId == categoryId)
                    .ToListAsync();

                // Check if any products were found
                if (products == null || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for category ID '{categoryId}'.";
                    return response;
                }

                // Map the retrieved products to response DTOs
                var productResponses = _mapper.Map<IEnumerable<ProductResponseDto>>(products);

                // Set successful response
                response.Success = true;
                response.Content = productResponses;
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

                // Retrieve products by name
                var products = await _unitOfWork.Repository
                    .GetQueryable<Product>(s => s.ProductName.Contains(productName))
                    .Include(p => p.ProductImages)
                    .ToListAsync();

                // Check if any products were found
                if (products == null || !products.Any())
                {
                    response.Success = false;
                    response.Title = "No Products Found";
                    response.Description = $"No products found for the search term '{productName}'.";
                    return response;
                }

                // Map the retrieved products to response DTOs
                var productResponses = _mapper.Map<IEnumerable<ProductResponseDto>>(products);

                // Set successful response
                response.Success = true;
                response.Content = productResponses;
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

        public async Task<ResponseModel<UserWishlistResponseDto>> AddProductToWishlistAsync(int productId,
            string userId, int shopId)
        {
            var response = new ResponseModel<UserWishlistResponseDto>();

            try
            {
                // Check if the product exists
                var product = await _unitOfWork.Repository
                    .GetQueryable<Product>(p => p.ProductId == productId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    response.Success = false;
                    response.Title = "Product Not Found";
                    response.Description = "The specified product does not exist.";
                    return response;
                }

                // Check if the wishlist item already exists for this user and product
                var existingWishlistItem = await _unitOfWork.Repository
                    .GetQueryable<UserWishlist>(w =>
                        w.ProductId == productId && w.UserId == userId && w.ShopId == shopId)
                    .FirstOrDefaultAsync();

                if (existingWishlistItem != null)
                {
                    response.Success = false;
                    response.Title = "Already in Wishlist";
                    response.Description = "This product is already in your wishlist.";
                    return response;
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


        public async Task<ResponseModel<string>> ChangeProductStatusAsync(int productId, ProductStatus newStatus)
        {
            var response = new ResponseModel<string>();
            var product = await _unitOfWork.Repository.GetQueryable<Product>(s => s.ProductId == productId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                response.Success = false;
                response.Title = "Product Not Found";
                response.Description = $"No shop found with ID {productId}.";
                return response;
            }

            product.Status = newStatus;

            try
            {
                // Save changes
                await _unitOfWork.Repository.UpdateAsync(product);
                await _unitOfWork.Repository.CompleteAsync();

                response.Success = true;
                response.Title = "Product Status Updated";
                response.Description = $"Product status successfully updated to {newStatus}.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Updating Product Status";
                response.Description = "An error occurred while updating the product status.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<List<ProductSaleResponseDto>>> GetSaleProductsAsync()
        {
            var response = new ResponseModel<List<ProductSaleResponseDto>>();

            try
            {
                var currentDate = DateTime.UtcNow;
                var saleProducts = await _unitOfWork.Repository
                    .GetQueryable<Product>(p => p.DiscountPercentage.HasValue &&
                                                p.SaleStartDate <= currentDate &&
                                                p.SaleEndDate >= currentDate)
                    .Include(p => p.ProductImages)
                    .ToListAsync();

                var saleProductDtos = saleProducts.Select(p => new ProductSaleResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    MainImage = p.ProductImages.FirstOrDefault()?.Path ?? string.Empty,
                    OriginalPrice = p.PricePerItem,
                    DiscountedPrice = p.PricePerItem - (p.PricePerItem * (p.DiscountPercentage.Value / 100)),
                    DiscountPercentage = p.DiscountPercentage.Value,
                    SaleEndDate = p.SaleEndDate.Value,
                    StockQuantity = p.StockQuantity,
                    TotalSold = p.totalsold ?? 0
                }).ToList();

                response.Success = true;
                response.Description = "Sale products retrieved successfully";
                response.Content = saleProductDtos;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Description = "Failed to retrieve sale products";
                response.Content = null!;
            }

            return response;
        }

        public async Task<List<ProductTopRatedDto>> GetTopRatedAffordableProductsAsync(string baseUrl)
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
                    Description = p.Description!,
                    PricePerItem = p.PricePerItem,
                    ShopImage = $"{baseUrl}{p.Shop.ShopLogo}"
                })
                .ToListAsync();

            return topRatedAffordableProducts;
        }
    }
}
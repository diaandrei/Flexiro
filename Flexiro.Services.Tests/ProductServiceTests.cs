using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Flexiro.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Flexiro.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly ProductService _productService;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IReviewService> _reviewServiceMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IBlobStorageService> _blobStorageServiceMock;

        public ProductServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _reviewServiceMock = new Mock<IReviewService>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _blobStorageServiceMock = new Mock<IBlobStorageService>();
            _productService = new ProductService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _reviewServiceMock.Object,
                _productRepositoryMock.Object,
                _blobStorageServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllCategoryNamesAsync_Success_ReturnsList()
        {
            var categoryNames = new List<string> { "CategoryA", "CategoryB" };
            _productRepositoryMock.Setup(repo => repo.GetAllCategoryNamesAsync())
                .ReturnsAsync(categoryNames);

            var result = await _productService.GetAllCategoryNamesAsync();

            Assert.True(result.Success);
            Assert.Equal(2, result.Content.Count);
            Assert.Equal("CategoryA", result.Content[0]);
        }

        [Fact]
        public async Task GetAllCategoryNamesAsync_NoCategories_ReturnsError()
        {
            _productRepositoryMock.Setup(repo => repo.GetAllCategoryNamesAsync())
                .ReturnsAsync(new List<string>());

            var result = await _productService.GetAllCategoryNamesAsync();

            Assert.False(result.Success);
            Assert.Equal("No Categories Found", result.Title);
        }

        [Fact]
        public async Task UpdateProductAsync_Success_ReturnsProductResponse()
        {
            var productId = 1;
            var productDto = new ProductUpdateDto { ProductName = "UpdatedName" };
            var updatedProduct = new Product { ProductId = 1, ProductName = "UpdatedName" };
            _productRepositoryMock.Setup(repo => repo.UpdateProductAsync(productId, productDto))
                .ReturnsAsync(updatedProduct);
            _mapperMock
                .Setup(m => m.Map<ProductResponseDto>(updatedProduct))
                .Returns(new ProductResponseDto
                {
                    ProductId = 1,
                    ProductName = "UpdatedName",
                    SKU = "XYZ",
                    Tags = new List<string>(),
                    ProductCondition = "New"
                });

            var result = await _productService.UpdateProductAsync(productId, productDto);

            Assert.True(result.Success);
            Assert.Equal("Product Updated Successfully", result.Title);
            Assert.Equal("UpdatedName", result.Content.ProductName);
        }

        [Fact]
        public async Task UpdateProductAsync_NotFound_ReturnsError()
        {
            var productId = 999;
            var productDto = new ProductUpdateDto();
            _productRepositoryMock.Setup(repo => repo.UpdateProductAsync(productId, productDto))
                .ReturnsAsync((Product)null);

            var result = await _productService.UpdateProductAsync(productId, productDto);

            Assert.False(result.Success);
            Assert.Equal("Product Not Found", result.Title);
        }

        [Fact]
        public async Task DeleteProductAsync_Success_ReturnsTrue()
        {
            var productId = 1;
            _productRepositoryMock.Setup(repo => repo.DeleteProductAsync(productId))
                .ReturnsAsync(true);

            var result = await _productService.DeleteProductAsync(productId);

            Assert.True(result);
        }

        [Fact]
        public async Task GetAllProductsAsync_Success_ReturnsProductLists()
        {
            var shopId = 10;
            var productList = new List<Product>
            {
                new Product { ProductId = 1, Status = ProductStatus.ForSell, DiscountPercentage = 0 },
                new Product { ProductId = 2, Status = ProductStatus.Draft },
                new Product { ProductId = 3, Status = ProductStatus.ForSell, DiscountPercentage = 10 },
                new Product { ProductId = 4, Status = ProductStatus.ForSell, Availability = AvailabilityStatus.NotForSale }
            };
            _productRepositoryMock.Setup(repo => repo.GetAllProductsAsync(shopId))
                .ReturnsAsync(productList);
            _productRepositoryMock.Setup(repo => repo.GetProductResponsesAsync(It.IsAny<List<Product>>()))
                .ReturnsAsync((List<Product> products) => products.Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName ?? "Name",
                    SKU = "SKU",
                    Tags = new List<string>(),
                    ProductCondition = "New"
                }).ToList());

            var result = await _productService.GetAllProductsAsync(shopId);

            Assert.True(result.Success);
            Assert.Equal("Products Retrieved Successfully", result.Title);
            Assert.NotNull(result.Content.ForSellProducts);
            Assert.NotNull(result.Content.DraftProducts);
            Assert.NotNull(result.Content.ForSaleProducts);
            Assert.NotNull(result.Content.NotForSaleProducts);
        }

        [Fact]
        public async Task GetAllProductsAsync_Error_ReturnsFailure()
        {
            var shopId = 10;
            _productRepositoryMock.Setup(repo => repo.GetAllProductsAsync(shopId))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _productService.GetAllProductsAsync(shopId);

            Assert.False(result.Success);
            Assert.Equal("Error Retrieving Products", result.Title);
        }

        [Fact]
        public async Task GetProductDetailsByIdAsync_Success_ReturnsDetails()
        {
            var productId = 1;
            var userId = "testUser";
            var productDetail = new ProductDetailResponseDto
            {
                ProductId = 1,
                ProductName = "ProdName",
                MainImage = "Image",
                ImageUrls = new List<string>(),
                Description = "Desc",
                CategoryName = "Cat",
                Reviews = new List<ReviewResponseDto>()
            };
            _productRepositoryMock.Setup(repo => repo.GetProductDetailsByIdAsync(productId, userId))
                .ReturnsAsync(productDetail);

            var result = await _productService.GetProductDetailsByIdAsync(productId, userId);

            Assert.True(result.Success);
            Assert.Equal("Product Retrieved Successfully", result.Title);
            Assert.NotNull(result.Content);
        }

        [Fact]
        public async Task GetProductDetailsByIdAsync_NotFound_ReturnsError()
        {
            var productId = 999;
            var userId = "testUser";
            _productRepositoryMock.Setup(repo => repo.GetProductDetailsByIdAsync(productId, userId))
                .ReturnsAsync((ProductDetailResponseDto)null);

            var result = await _productService.GetProductDetailsByIdAsync(productId, userId);

            Assert.False(result.Success);
            Assert.Equal("Product Not Found", result.Title);
        }

        [Fact]
        public async Task GetProductsByShopIdAsync_Success_ReturnsList()
        {
            var shopId = 10;
            var userId = "testUser";
            var products = new List<ProductResponse>
            {
                new ProductResponse
                {
                    ProductId = 1,
                    ProductName = "Name",
                    MainImage = "Img",
                    ImageUrls = new List<string>(),
                    Description = "Desc",
                    CategoryName = "Cat"
                }
            };
            _productRepositoryMock.Setup(repo => repo.GetProductsByShopIdAsync(shopId, userId))
                .ReturnsAsync(products);

            var result = await _productService.GetProductsByShopIdAsync(shopId, userId);

            Assert.True(result.Success);
            Assert.Single(result.Content);
            Assert.Equal("Products Retrieved Successfully", result.Title);
        }

        [Fact]
        public async Task GetProductsByShopIdAsync_NoProducts_ReturnsError()
        {
            var shopId = 10;
            var userId = "testUser";
            _productRepositoryMock.Setup(repo => repo.GetProductsByShopIdAsync(shopId, userId))
                .ReturnsAsync(new List<ProductResponse>());

            var result = await _productService.GetProductsByShopIdAsync(shopId, userId);

            Assert.False(result.Success);
            Assert.Equal("No Products Found", result.Title);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_Success_ReturnsProducts()
        {
            var categoryId = 100;
            var productList = new List<ProductResponseDto>
            {
                new ProductResponseDto
                {
                    ProductId = 1,
                    ProductName = "Name",
                    SKU = "SKU",
                    Tags = new List<string>(),
                    ProductCondition = "Condition"
                }
            };
            _productRepositoryMock.Setup(repo => repo.GetProductsByCategoryIdAsync(categoryId))
                .ReturnsAsync(productList);

            var result = await _productService.GetProductsByCategoryIdAsync(categoryId);

            Assert.True(result.Success);
            Assert.Single(result.Content);
            Assert.Equal("Products Retrieved Successfully", result.Title);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_NoProducts_ReturnsError()
        {
            var categoryId = 999;
            _productRepositoryMock.Setup(repo => repo.GetProductsByCategoryIdAsync(categoryId))
                .ReturnsAsync(new List<ProductResponseDto>());

            var result = await _productService.GetProductsByCategoryIdAsync(categoryId);

            Assert.False(result.Success);
            Assert.Equal("No Products Found", result.Title);
        }

        [Fact]
        public async Task SearchProductsByNameAsync_Success_ReturnsProducts()
        {
            var productName = "Test";
            var productList = new List<ProductResponseDto>
            {
                new ProductResponseDto
                {
                    ProductId = 1,
                    ProductName = "Name",
                    SKU = "SKU",
                    Tags = new List<string>(),
                    ProductCondition = "New"
                }
            };
            _productRepositoryMock.Setup(repo => repo.SearchProductsByNameAsync(productName))
                .ReturnsAsync(productList);

            var result = await _productService.SearchProductsByNameAsync(productName);

            Assert.True(result.Success);
            Assert.Single(result.Content);
            Assert.Equal("Products Retrieved Successfully", result.Title);
        }

        [Fact]
        public async Task SearchProductsByNameAsync_EmptyName_ReturnsError()
        {
            var productName = "";

            var result = await _productService.SearchProductsByNameAsync(productName);

            Assert.False(result.Success);
            Assert.Equal("Invalid Search Term", result.Title);
        }

        [Fact]
        public async Task SearchProductsByNameAsync_NoProducts_ReturnsError()
        {
            var productName = "Unknown";
            _productRepositoryMock.Setup(repo => repo.SearchProductsByNameAsync(productName))
                .ReturnsAsync(new List<ProductResponseDto>());

            var result = await _productService.SearchProductsByNameAsync(productName);

            Assert.False(result.Success);
            Assert.Equal("No Products Found", result.Title);
        }

        [Fact]
        public async Task AddProductToWishlistAsync_Success_ReturnsWishlistItem()
        {
            var productId = 1;
            var userId = "testUser";
            var shopId = 10;
            var wishItem = new UserWishlist { ProductId = productId };
            _productRepositoryMock.Setup(repo => repo.AddProductToWishlistAsync(productId, userId, shopId))
                .ReturnsAsync(wishItem);
            _mapperMock.Setup(m => m.Map<UserWishlistResponseDto>(wishItem))
                .Returns(new UserWishlistResponseDto
                {
                    ProductId = productId,
                    UserId = "testUser"
                });

            var result = await _productService.AddProductToWishlistAsync(productId, userId, shopId);

            Assert.True(result.Success);
            Assert.Equal("Product Added to Wishlist", result.Title);
            Assert.Equal(productId, result.Content.ProductId);
            Assert.Equal("testUser", result.Content.UserId);
        }

        [Fact]
        public async Task AddProductToWishlistAsync_Failure_ReturnsError()
        {
            var productId = 999;
            var userId = "testUser";
            var shopId = 10;
            _productRepositoryMock.Setup(repo => repo.AddProductToWishlistAsync(productId, userId, shopId))
                .ReturnsAsync((UserWishlist)null);

            var result = await _productService.AddProductToWishlistAsync(productId, userId, shopId);

            Assert.False(result.Success);
            Assert.Equal("Error Adding to Wishlist", result.Title);
        }

        [Fact]
        public async Task RemoveProductFromWishlistAsync_Success_RemovesItem()
        {
            var productId = 1;
            var userId = "testUser";
            var shopId = 10;
            _productRepositoryMock.Setup(repo => repo.RemoveProductFromWishlistAsync(productId, userId))
                .ReturnsAsync(true);

            var result = await _productService.RemoveProductFromWishlistAsync(productId, userId, shopId);

            Assert.True(result.Success);
            Assert.Equal("Product Removed from Wishlist", result.Title);
        }

        [Fact]
        public async Task RemoveProductFromWishlistAsync_Failure_ReturnsError()
        {
            var productId = 999;
            var userId = "testUser";
            var shopId = 10;
            _productRepositoryMock.Setup(repo => repo.RemoveProductFromWishlistAsync(productId, userId))
                .ReturnsAsync(false);

            var result = await _productService.RemoveProductFromWishlistAsync(productId, userId, shopId);

            Assert.False(result.Success);
            Assert.Equal("Error Removing from Wishlist", result.Title);
        }

        [Fact]
        public async Task ChangeProductStatusAsync_Success_UpdatesStatus()
        {
            var productId = 1;
            var newStatus = (int)ProductStatus.SoldOut;
            _productRepositoryMock.Setup(repo => repo.ChangeProductStatusAsync(productId, newStatus))
                .ReturnsAsync(new Product { ProductId = productId });

            var result = await _productService.ChangeProductStatusAsync(productId, newStatus);

            Assert.True(result.Success);
            Assert.Equal("Product Status Updated", result.Title);
        }

        [Fact]
        public async Task ChangeProductStatusAsync_Failure_ReturnsError()
        {
            var productId = 999;
            var newStatus = (int)ProductStatus.SoldOut;
            _productRepositoryMock.Setup(repo => repo.ChangeProductStatusAsync(productId, newStatus))
                .ReturnsAsync((Product)null);

            var result = await _productService.ChangeProductStatusAsync(productId, newStatus);

            Assert.False(result.Success);
            Assert.Equal("Error Updating Product Status", result.Title);
        }

        [Fact]
        public async Task GetSaleProductsAsync_NoProducts_ReturnsError()
        {
            _productRepositoryMock.Setup(repo => repo.GetSaleProductsAsync())
                .ReturnsAsync(new List<ProductSaleResponseDto>());

            var result = await _productService.GetSaleProductsAsync();

            Assert.False(result.Success);
            Assert.Equal("No Sale Products Found", result.Title);
        }

        [Fact]
        public async Task GetWishlistProductsByUserAsync_NoItems_ReturnsError()
        {
            var userId = "testUser";
            _productRepositoryMock
                .Setup(repo => repo.GetWishlistProductsByUserAsync(userId))
                .ReturnsAsync(new List<UserWishlist>());

            var result = await _productService.GetWishlistProductsByUserAsync(userId);

            Assert.False(result.Success);
            Assert.Equal("No Wishlist Items Found", result.Title);
        }

        [Fact]
        public async Task GetWishlistProductsByShopAsync_NoItems_ReturnsError()
        {
            var shopId = 10;
            _productRepositoryMock
                .Setup(repo => repo.GetWishlistProductsByShopAsync(shopId))
                .ReturnsAsync(new List<UserWishlist>());

            var result = await _productService.GetWishlistProductsByShopAsync(shopId);

            Assert.False(result.Success);
            Assert.Equal("No items found in your wishlist", result.Title);
        }

        [Fact]
        public async Task AddOrUpdateDiscountPercentageAsync_Success_UpdatesDiscount()
        {
            var productId = 1;
            var discountDto = new UpdateDiscountDto { DiscountPercentage = 20 };
            _productRepositoryMock
                .Setup(repo => repo.AddOrUpdateDiscountPercentageAsync(productId, 20))
                .ReturnsAsync(true);

            var result = await _productService.AddOrUpdateDiscountPercentageAsync(productId, discountDto);

            Assert.True(result.Success);
            Assert.Equal("Discount Updated Successfully", result.Title);
        }

        [Fact]
        public async Task AddOrUpdateDiscountPercentageAsync_InvalidDiscount_ReturnsError()
        {
            var productId = 1;
            var discountDto = new UpdateDiscountDto { DiscountPercentage = 110 };

            var result = await _productService.AddOrUpdateDiscountPercentageAsync(productId, discountDto);

            Assert.False(result.Success);
            Assert.Equal("Invalid Discount Percentage", result.Title);
        }

        [Fact]
        public async Task AddOrUpdateDiscountPercentageAsync_Failure_ProductNotFound()
        {
            var productId = 999;
            var discountDto = new UpdateDiscountDto { DiscountPercentage = 20 };
            _productRepositoryMock
                .Setup(repo => repo.AddOrUpdateDiscountPercentageAsync(productId, 20))
                .ReturnsAsync(false);

            var result = await _productService.AddOrUpdateDiscountPercentageAsync(productId, discountDto);

            Assert.False(result.Success);
            Assert.Equal("Product Not Found", result.Title);
        }
    }
}

using AutoMapper;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Flexiro.Tests.Services
{
    public class CartServiceTests
    {
        private readonly CartService _cartService;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CartService>> _loggerMock;
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;

        public CartServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CartService>>();
            _cartRepositoryMock = new Mock<ICartRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _cartService = new CartService(
                null,
                _mapperMock.Object,
                _loggerMock.Object,
                _cartRepositoryMock.Object,
                _productRepositoryMock.Object);
        }

        [Fact]
        public async Task AddItemToCartAsync_ProductExists_Succeeds()
        {
            // Arrange
            var userId = "testUser";
            var requestModel = new MultiCartItemRequestModel
            {
                IsGuest = false,
                Items = new List<CartItemRequestModel>
                {
                    new CartItemRequestModel { ProductId = 1, Quantity = 2, ShopId = 10 }
                }
            };
            _cartRepositoryMock.Setup(repo => repo.GetCartByUserIdAsync(userId)).ReturnsAsync((Cart)null);
            var newCart = new Cart
            {
                UserId = userId,
                GuestUserId = null,
                CartItems = new List<CartItem>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _cartRepositoryMock.Setup(repo => repo.CreateNewCartAsync(userId, false)).ReturnsAsync(newCart);
            var product = new Product
            {
                ProductId = 1,
                StockQuantity = 10,
                PricePerItem = 100m,
                DiscountPercentage = 10
            };
            _productRepositoryMock.Setup(repo => repo.GetProductByIdAsync(1)).ReturnsAsync(product);
            var cartItem = new CartItem
            {
                CartItemId = 100,
                ProductId = 1,
                ShopId = 10,
                Quantity = 2,
                PricePerUnit = 90m,
                DiscountAmount = 10m * 2,
                TotalPrice = 90m * 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _cartRepositoryMock
                .Setup(repo => repo.AddOrUpdateCartItemAsync(newCart, requestModel.Items[0], product))
                .ReturnsAsync(cartItem);
            _cartRepositoryMock.Setup(repo => repo.UpdateCartTotalsAsync(newCart)).Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddItemToCartAsync(requestModel, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Content);
            Assert.Equal("Item Added Successfully", result.Title);
            _cartRepositoryMock.Verify(repo => repo.CreateNewCartAsync(userId, false), Times.Once);
            _cartRepositoryMock.Verify(repo => repo.AddOrUpdateCartItemAsync(newCart, requestModel.Items[0], product), Times.Once);
        }

        [Fact]
        public async Task AddItemToCartAsync_ProductNotFound_Fails()
        {
            // Arrange
            var userId = "testUser";
            var requestModel = new MultiCartItemRequestModel
            {
                IsGuest = false,
                Items = new List<CartItemRequestModel>
                {
                    new CartItemRequestModel { ProductId = 99, Quantity = 1, ShopId = 10 }
                }
            };
            var existingCart = new Cart
            {
                UserId = userId,
                GuestUserId = null,
                CartItems = new List<CartItem>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _cartRepositoryMock.Setup(repo => repo.GetCartByUserIdAsync(userId)).ReturnsAsync(existingCart);
            _productRepositoryMock.Setup(repo => repo.GetProductByIdAsync(99)).ReturnsAsync((Product)null);

            // Act
            var result = await _cartService.AddItemToCartAsync(requestModel, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Product Not Found", result.Title);
        }

        [Fact]
        public async Task GetCartAsync_CartExists_ReturnsSuccess()
        {
            // Arrange
            var userId = "testUser";
            var mainCartModel = new MainCartModel
            {
                CartId = 1,
                Items = new List<CartItemDetailModel>(),
                SubTotal = 0,
                TotalAmount = 0,
                Discount = 0,
                Tax = 0,
                ShippingCost = 0,
                CreatedAt = DateTime.UtcNow
            };
            _cartRepositoryMock.Setup(repo => repo.GetCartAsync(userId)).ReturnsAsync(mainCartModel);

            // Act
            var result = await _cartService.GetCartAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Content);
            Assert.Equal("Cart Retrieved Successfully", result.Title);
        }

        [Fact]
        public async Task GetCartAsync_CartNotFound_ReturnsError()
        {
            // Arrange
            var userId = "nonexistentUser";
            _cartRepositoryMock.Setup(repo => repo.GetCartAsync(userId)).ReturnsAsync((MainCartModel)null);

            // Act
            var result = await _cartService.GetCartAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cart Not Found", result.Title);
        }
    }
}

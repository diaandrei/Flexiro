using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Flexiro.Services.Services.Interfaces;
using Moq;

namespace Flexiro.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly OrderService _orderService;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Mock<IShippingRepository> _shippingRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;

        public OrderServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _cartRepositoryMock = new Mock<ICartRepository>();
            _shippingRepositoryMock = new Mock<IShippingRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _orderService = new OrderService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _orderRepositoryMock.Object,
                _cartRepositoryMock.Object,
                _shippingRepositoryMock.Object,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public async Task PlaceOrderAsync_SuccessfulOrder_ReturnsOrderResponse()
        {
            // Arrange
            var userId = "testUser";
            var shippingAddressRequest = new AddUpdateShippingAddressRequest
            {
                Address = "123 St",
                City = "CityA",
                Country = "CountryA",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Note = "Handle with care",
                PhoneNumber = "1234567890",
                Postcode = "12345"
            };
            var order = new Order
            {
                OrderId = 1,
                OrderNumber = "ORD123",
                ItemsTotal = 100,
                ShippingCost = 5,
                Tax = 2,
                TotalAmount = 107,
                Status = OrderStatus.New,
                CreatedAt = DateTime.UtcNow,
                ShippingAddress = new ShippingAddress { Address = "123 St" },
                OrderDetails = new List<OrderDetails>
                {
                    new OrderDetails { ProductId = 1, Quantity = 1, PricePerUnit = 100, DiscountAmount = 0 }
                }
            };
            _orderRepositoryMock
                .Setup(repo => repo.CreateOrderAsync(userId, shippingAddressRequest, "Cash"))
                .ReturnsAsync(order);
            _mapperMock
                .Setup(m => m.Map<ShippingAddressResponseDto>(order.ShippingAddress))
                .Returns(new ShippingAddressResponseDto { Address = "123 St" });

            // Act
            var response = await _orderService.PlaceOrderAsync(userId, shippingAddressRequest, "Cash");

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Content);
            Assert.Equal("Order Placed Successfully", response.Title);
            Assert.Equal("ORD123", response.Content.OrderNumber);
        }

        [Fact]
        public async Task PlaceOrderAsync_NullOrder_ReturnsError()
        {
            // Arrange
            var userId = "testUser";
            var shippingAddressRequest = new AddUpdateShippingAddressRequest
            {
                Address = "Any St",
                City = "CityB",
                Country = "CountryB",
                Email = "any@example.com",
                FirstName = "Any",
                LastName = "One",
                Note = "Note",
                PhoneNumber = "9876543210",
                Postcode = "54321"
            };
            _orderRepositoryMock
                .Setup(repo => repo.CreateOrderAsync(userId, shippingAddressRequest, "Card"))
                .ReturnsAsync((Order)null);

            // Act
            var response = await _orderService.PlaceOrderAsync(userId, shippingAddressRequest, "Card");

            // Assert
            Assert.False(response.Success);
            Assert.Equal("Cart is empty", response.Title);
        }

        [Fact]
        public async Task GetTotalOrdersByShopAsync_ReturnsCount()
        {
            // Arrange
            var shopId = 10;
            _orderRepositoryMock.Setup(repo => repo.GetTotalOrdersByShopAsync(shopId)).ReturnsAsync(5);

            // Act
            var result = await _orderService.GetTotalOrdersByShopAsync(shopId);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task GetDeliveredOrdersByShopAsync_ReturnsListAndCount()
        {
            // Arrange
            var shopId = 10;
            var orders = new List<Order> { new Order { OrderId = 1, Status = OrderStatus.Delivered } };
            _orderRepositoryMock
                .Setup(repo => repo.GetDeliveredOrdersByShopAsync(shopId))
                .ReturnsAsync((orders, orders.Count));

            // Act
            var (list, count) = await _orderService.GetDeliveredOrdersByShopAsync(shopId);

            // Assert
            Assert.Single(list);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetAllCustomersByShopAsync_ReturnsListAndCount()
        {
            // Arrange
            var shopId = 10;
            var customers = new List<string> { "user1", "user2" };
            _orderRepositoryMock
                .Setup(repo => repo.GetAllCustomersByShopAsync(shopId))
                .ReturnsAsync((customers, customers.Count));

            // Act
            var (list, count) = await _orderService.GetAllCustomersByShopAsync(shopId);

            // Assert
            Assert.Equal(2, count);
            Assert.Equal("user1", list[0]);
            Assert.Equal("user2", list[1]);
        }

        [Fact]
        public async Task GetTotalEarningsByShopAsync_ReturnsValue()
        {
            // Arrange
            var shopId = 10;
            _orderRepositoryMock.Setup(repo => repo.GetTotalEarningsByShopAsync(shopId)).ReturnsAsync(150.50m);

            // Act
            var result = await _orderService.GetTotalEarningsByShopAsync(shopId);

            // Assert
            Assert.Equal(150.50m, result);
        }

        [Fact]
        public async Task GetNewOrderCountByShopAsync_ReturnsValue()
        {
            // Arrange
            var shopId = 10;
            _orderRepositoryMock.Setup(repo => repo.GetNewOrderCountByShopAsync(shopId)).ReturnsAsync(3);

            // Act
            var result = await _orderService.GetNewOrderCountByShopAsync(shopId);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public async Task GetOrdersByCustomerAsync_ReturnsOrders()
        {
            // Arrange
            var userId = "testUser";
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = 1,
                    OrderNumber = "ORD100",
                    ItemsTotal = 50,
                    ShippingCost = 5,
                    Tax = 2,
                    TotalAmount = 57,
                    Status = OrderStatus.New,
                    PaymentStatus = "Paid",
                    PaymentMethod = "Card",
                    CreatedAt = DateTime.UtcNow,
                    ShippingAddress = new ShippingAddress { Address = "123 Lane", City = "CityX", Postcode = "11111" },
                    OrderDetails = new List<OrderDetails>
                    {
                        new OrderDetails
                        {
                            ProductId = 10,
                            Product = new Product { ProductName = "ProdX" },
                            Quantity = 1,
                            PricePerUnit = 50,
                            DiscountAmount = 0,
                            TotalPrice = 50
                        }
                    }
                }
            };
            _orderRepositoryMock.Setup(repo => repo.GetOrdersByCustomerAsync(userId)).ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetOrdersByCustomerAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("ORD100", result[0].OrderNumber);
            Assert.Single(result[0].OrderItems);
        }
    }
}

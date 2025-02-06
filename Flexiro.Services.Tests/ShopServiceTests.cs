using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Flexiro.Services.Services.Interfaces;
using Moq;

namespace Flexiro.Tests.Services
{
    public class ShopServiceTests
    {
        private readonly ShopService _shopService;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IShopRepository> _shopRepositoryMock;
        private readonly Mock<IBlobStorageService> _blobStorageServiceMock;

        public ShopServiceTests()
        {
            // arrange
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _shopRepositoryMock = new Mock<IShopRepository>();
            _blobStorageServiceMock = new Mock<IBlobStorageService>();

            _shopService = new ShopService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _shopRepositoryMock.Object,
                _blobStorageServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateShopAsync_Success()
        {
            // arrange
            var shopRequest = new Shop { ShopName = "Test Shop" };
            var createdShop = new Shop { ShopId = 1, ShopName = "Test Shop" };
            _shopRepositoryMock
                .Setup(repo => repo.CreateShopAsync(shopRequest))
                .ReturnsAsync(createdShop);

            // act
            var result = await _shopService.CreateShopAsync(shopRequest);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Shop Created Successfully", result.Title);
            Assert.NotNull(result.Content);
            Assert.Equal("Test Shop", result.Content.ShopName);
        }

        [Fact]
        public async Task CreateShopAsync_Error()
        {
            // arrange
            var shopRequest = new Shop { ShopName = "Test Shop" };
            _shopRepositoryMock
                .Setup(repo => repo.CreateShopAsync(shopRequest))
                .ThrowsAsync(new Exception("DB error"));

            // act
            var result = await _shopService.CreateShopAsync(shopRequest);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Error Creating Shop", result.Title);
        }

        [Fact]
        public async Task UpdateShopAsync_Success()
        {
            // arrange
            var updateRequest = new UpdateShopRequest { ShopId = 1, ShopName = "Updated Name" };
            var updatedShop = new Shop { ShopId = 1, ShopName = "Updated Name" };
            _shopRepositoryMock
                .Setup(repo => repo.UpdateShopAsync(updateRequest))
                .ReturnsAsync(updatedShop);

            // act
            var result = await _shopService.UpdateShopAsync(updateRequest);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Shop Updated Successfully", result.Title);
            Assert.NotNull(result.Content);
            Assert.Equal("Updated Name", result.Content.ShopName);
        }

        [Fact]
        public async Task UpdateShopAsync_ShopNotFound()
        {
            // arrange
            var updateRequest = new UpdateShopRequest { ShopId = 999 };
            _shopRepositoryMock
                .Setup(repo => repo.UpdateShopAsync(updateRequest))
                .ThrowsAsync(new KeyNotFoundException("Shop not found"));

            // act
            var result = await _shopService.UpdateShopAsync(updateRequest);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Shop Not Found", result.Title);
        }

        [Fact]
        public async Task GetShopByIdAsync_Success()
        {
            // arrange
            var shopId = 10;
            var shop = new Shop { ShopId = 10, ShopName = "ShopName" };
            _shopRepositoryMock
                .Setup(repo => repo.GetShopByIdAsync(shopId))
                .ReturnsAsync(shop);

            // act
            var result = await _shopService.GetShopByIdAsync(shopId);

            // assert
            Assert.True(result.Success);
            Assert.NotNull(result.Content);
            Assert.Equal("Shop Retrieved Successfully", result.Title);
            Assert.Equal(10, result.Content.ShopId);
        }

        [Fact]
        public async Task GetShopByIdAsync_NotFound()
        {
            // arrange
            var shopId = 999;
            _shopRepositoryMock
                .Setup(repo => repo.GetShopByIdAsync(shopId))
                .ReturnsAsync((Shop)null);

            // act
            var result = await _shopService.GetShopByIdAsync(shopId);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Shop Not Found", result.Title);
        }

        [Fact]
        public async Task GetActiveShopsAsync_Success()
        {
            // arrange
            var shops = new List<Shop> { new Shop { ShopId = 1 }, new Shop { ShopId = 2 } };
            _shopRepositoryMock
                .Setup(repo => repo.GetActiveShopsAsync())
                .ReturnsAsync(shops);

            // act
            var result = await _shopService.GetActiveShopsAsync();

            // assert
            Assert.True(result.Success);
            Assert.Equal("Active Shops Retrieved Successfully", result.Title);
            Assert.Equal(2, result.Content.Count);
        }

        [Fact]
        public async Task GetActiveShopsAsync_Error()
        {
            // arrange
            _shopRepositoryMock
                .Setup(repo => repo.GetActiveShopsAsync())
                .ThrowsAsync(new Exception("DB error"));

            // act
            var result = await _shopService.GetActiveShopsAsync();

            // assert
            Assert.False(result.Success);
            Assert.Equal("Error Retrieving Active Shops", result.Title);
        }

        [Fact]
        public async Task GetPendingShopsAsync_Success()
        {
            // arrange
            var shops = new List<Shop> { new Shop { ShopId = 3 } };
            _shopRepositoryMock
                .Setup(repo => repo.GetPendingShopsAsync())
                .ReturnsAsync(shops);

            // act
            var result = await _shopService.GetPendingShopsAsync();

            // assert
            Assert.True(result.Success);
            Assert.Equal("Pending Shops Retrieved Successfully", result.Title);
            Assert.Single(result.Content);
        }

        [Fact]
        public async Task GetInactiveShopsAsync_Success()
        {
            // arrange
            var shops = new List<Shop> { new Shop { ShopId = 4 } };
            _shopRepositoryMock
                .Setup(repo => repo.GetInactiveShopsAsync())
                .ReturnsAsync(shops);

            // act
            var result = await _shopService.GetInactiveShopsAsync();

            // assert
            Assert.True(result.Success);
            Assert.Equal("Inactive Shops Retrieved Successfully", result.Title);
            Assert.Single(result.Content);
        }

        [Fact]
        public async Task GetAllShopsAsync_Success()
        {
            // arrange
            var shops = new List<Shop> { new Shop { ShopId = 5 }, new Shop { ShopId = 6 } };
            _shopRepositoryMock
                .Setup(repo => repo.GetAllShopsAsync())
                .ReturnsAsync(shops);

            // act
            var result = await _shopService.GetAllShopsAsync();

            // assert
            Assert.True(result.Success);
            Assert.Equal("All Shops Retrieved Successfully", result.Title);
            Assert.Equal(2, result.Content.Count);
        }

        [Fact]
        public async Task SearchShopsByNameAsync_Success()
        {
            // arrange
            var shopName = "Test";
            var shops = new List<Shop> { new Shop { ShopId = 7, ShopName = "TestShop" } };
            _shopRepositoryMock
                .Setup(repo => repo.SearchShopsByNameAsync(shopName))
                .ReturnsAsync(shops);

            // act
            var result = await _shopService.SearchShopsByNameAsync(shopName);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Shops Retrieved Successfully", result.Title);
            Assert.Single(result.Content);
        }

        [Fact]
        public async Task UpdateShopStatusAsync_Success()
        {
            // arrange
            var shopId = 1;
            var newStatus = ShopAdminStatus.Active;
            var updatedShop = new Shop { ShopId = 1, ShopName = "Updated", AdminStatus = ShopAdminStatus.Active };
            _shopRepositoryMock.Setup(repo => repo.UpdateShopStatusAsync(shopId, newStatus))
                .ReturnsAsync(updatedShop);

            // act
            var result = await _shopService.UpdateShopStatusAsync(shopId, newStatus);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Shop Status Updated Successfully", result.Title);
            Assert.NotNull(result.Content);
            Assert.Equal(ShopAdminStatus.Active, result.Content.AdminStatus);
        }

        [Fact]
        public async Task UpdateShopStatusAsync_NotFound()
        {
            // arrange
            var shopId = 999;
            var newStatus = ShopAdminStatus.Active;
            _shopRepositoryMock.Setup(repo => repo.UpdateShopStatusAsync(shopId, newStatus))
                .ReturnsAsync((Shop)null);

            // act
            var result = await _shopService.UpdateShopStatusAsync(shopId, newStatus);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Shop Not Found", result.Title);
        }

        [Fact]
        public async Task ChangeShopStatusByAdminAsync_Success()
        {
            // arrange
            var shopId = 1;
            var newStatus = 2;
            _shopRepositoryMock.Setup(repo => repo.ChangeShopStatusAsync(shopId, newStatus))
                .ReturnsAsync(true);

            // act
            var result = await _shopService.ChangeShopStatusByAdminAsync(shopId, newStatus);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Shop Status Updated", result.Title);
        }

        [Fact]
        public async Task ChangeShopStatusByAdminAsync_NotFound()
        {
            // arrange
            var shopId = 999;
            var newStatus = 2;
            _shopRepositoryMock.Setup(repo => repo.ChangeShopStatusAsync(shopId, newStatus))
                .ReturnsAsync(false);

            // act
            var result = await _shopService.ChangeShopStatusByAdminAsync(shopId, newStatus);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Shop Not Found", result.Title);
        }

        [Fact]
        public async Task ChangeShopSellerStatusAsync_Success()
        {
            // arrange
            var shopStatus = new ShopStatus { ShopId = 1 };
            _shopRepositoryMock.Setup(repo => repo.UpdateShopSellerStatusAsync(shopStatus))
                .ReturnsAsync(true);

            // act
            var result = await _shopService.ChangeShopSellerStatusAsync(shopStatus);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Shop Status Updated Successfully", result.Title);
        }

        [Fact]
        public async Task ChangeShopSellerStatusAsync_NotFound()
        {
            // arrange
            var shopStatus = new ShopStatus { ShopId = 999 };
            _shopRepositoryMock.Setup(repo => repo.UpdateShopSellerStatusAsync(shopStatus))
                .ReturnsAsync(false);

            // act
            var result = await _shopService.ChangeShopSellerStatusAsync(shopStatus);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Shop Not Found", result.Title);
        }
    }
}

using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Moq;

namespace Flexiro.Tests.Services
{
    public class ShippingServiceTests
    {
        private readonly ShippingService _shippingService;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IShippingRepository> _shippingRepositoryMock;

        public ShippingServiceTests()
        {
            // arrange
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _shippingRepositoryMock = new Mock<IShippingRepository>();

            _shippingService = new ShippingService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _shippingRepositoryMock.Object
            );
        }

        [Fact]
        public async Task GetShippingAddressByUserIdAsync_Success()
        {
            // arrange
            var userId = "testUser";
            var address = new ShippingAddress { UserId = userId };
            _shippingRepositoryMock
                .Setup(repo => repo.GetShippingAddressByUserIdAsync(userId))
                .ReturnsAsync(address);

            // act
            var result = await _shippingService.GetShippingAddressByUserIdAsync(userId);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Address Retrieved Successfully", result.Title);
            Assert.NotNull(result.Content);
            Assert.Equal(userId, result.Content.UserId);
        }

        [Fact]
        public async Task GetShippingAddressByUserIdAsync_NotFound()
        {
            // arrange
            var userId = "unknownUser";
            _shippingRepositoryMock
                .Setup(repo => repo.GetShippingAddressByUserIdAsync(userId))
                .ReturnsAsync((ShippingAddress)null);

            // act
            var result = await _shippingService.GetShippingAddressByUserIdAsync(userId);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Address Not Found", result.Title);
        }

        [Fact]
        public async Task AddShippingAddressAsync_Success()
        {
            // arrange
            var request = new AddUpdateShippingAddressRequest
            {
                Address = "123 Main St",
                City = "TestCity",
                Country = "TestCountry",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Note = "Leave at door",
                PhoneNumber = "1234567890",
                Postcode = "11111"
            };
            var newAddress = new ShippingAddress
            {
                UserId = "testUser",
                Address = "123 Main St",
                City = "TestCity",
                Country = "TestCountry"
            };
            _shippingRepositoryMock
                .Setup(repo => repo.AddShippingAddressAsync(request))
                .ReturnsAsync(newAddress);

            // act
            var result = await _shippingService.AddShippingAddressAsync(request);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Address Added Successfully", result.Title);
            Assert.NotNull(result.Content);
            Assert.Equal("TestCity", result.Content.City);
        }

        [Fact]
        public async Task UpdateShippingAddressAsync_Success()
        {
            // arrange
            var addressId = 1;
            var request = new AddUpdateShippingAddressRequest
            {
                Address = "234 Another St",
                City = "NewCity",
                Country = "NewCountry",
                Email = "test2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Note = "Ring bell",
                PhoneNumber = "9876543210",
                Postcode = "22222"
            };
            var updated = new ShippingAddress
            {
                UserId = "testUser",
                Address = "234 Another St",
                City = "NewCity",
                Country = "NewCountry"
            };
            _shippingRepositoryMock
                .Setup(repo => repo.UpdateShippingAddressAsync(addressId, request))
                .ReturnsAsync(updated);

            // act
            var result = await _shippingService.UpdateShippingAddressAsync(addressId, request);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Address Updated Successfully", result.Title);
            Assert.NotNull(result.Content);
            Assert.Equal("NewCity", result.Content.City);
        }

        [Fact]
        public async Task UpdateShippingAddressAsync_NotFound()
        {
            // arrange
            var addressId = 999;
            var request = new AddUpdateShippingAddressRequest
            {
                Address = "Xyz St",
                City = "NoCity",
                Country = "NoCountry",
                Email = "no@example.com",
                FirstName = "None",
                LastName = "None",
                Note = "N/A",
                PhoneNumber = "0000000000",
                Postcode = "00000"
            };
            _shippingRepositoryMock
                .Setup(repo => repo.UpdateShippingAddressAsync(addressId, request))
                .ReturnsAsync((ShippingAddress)null);

            // act
            var result = await _shippingService.UpdateShippingAddressAsync(addressId, request);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Address Not Found", result.Title);
        }

        [Fact]
        public async Task GetAddressBookByUserIdAsync_Success()
        {
            // arrange
            var userId = "testUser";
            var addresses = new List<ShippingAddress>
            {
                new ShippingAddress { UserId = "testUser", Address = "X St" }
            };
            _shippingRepositoryMock
                .Setup(repo => repo.GetAddressBookByUserIdAsync(userId))
                .ReturnsAsync(addresses);

            // act
            var result = await _shippingService.GetAddressBookByUserIdAsync(userId);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Address Book Retrieved Successfully", result.Title);
            Assert.Single(result.Content);
            Assert.Equal("X St", result.Content[0].Address);
        }
    }
}

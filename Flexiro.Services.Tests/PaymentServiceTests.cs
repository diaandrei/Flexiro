using Flexiro.Application.Models;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Moq;

namespace Flexiro.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly PaymentService _paymentService;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;

        public PaymentServiceTests()
        {
            // arrange
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _paymentService = new PaymentService(_paymentRepositoryMock.Object);
        }

        [Fact]
        public async Task SavePaymentAsync_Success()
        {
            // arrange
            var paymentRequest = new Payment { PaymentId = 1, OrderId = 100 };
            var savedPayment = new Payment { PaymentId = 1, OrderId = 100 };
            _paymentRepositoryMock
                .Setup(repo => repo.SavePaymentAsync(paymentRequest))
                .ReturnsAsync(savedPayment);

            // act
            var result = await _paymentService.SavePaymentAsync(paymentRequest);

            // assert
            Assert.True(result.Success);
            Assert.Equal("Payment Saved Successfully", result.Title);
            Assert.NotNull(result.Content);
            Assert.Equal(100, result.Content.OrderId);
        }

        [Fact]
        public async Task SavePaymentAsync_Error()
        {
            // arrange
            var paymentRequest = new Payment { PaymentId = 2, OrderId = 200 };
            _paymentRepositoryMock
                .Setup(repo => repo.SavePaymentAsync(paymentRequest))
                .ThrowsAsync(new Exception("DB error"));

            // act
            var result = await _paymentService.SavePaymentAsync(paymentRequest);

            // assert
            Assert.False(result.Success);
            Assert.Equal("Error Saving Payment", result.Title);
            Assert.Contains("DB error", result.ExceptionMessage);
        }
    }
}

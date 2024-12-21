using Flexiro.Application.Models;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;

namespace Flexiro.Services.Services
{

    public class PaymentService : IPaymentService
    {

        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<ResponseModel<Payment>> SavePaymentAsync(Payment paymentRequest)
        {
            var response = new ResponseModel<Payment>();

            try
            {
                // Call the repository method to save the payment
                var savedPayment = await _paymentRepository.SavePaymentAsync(paymentRequest);

                // Set successful response
                response.Success = true;
                response.Content = savedPayment;
                response.Title = "Payment Saved Successfully";
                response.Description = $"Payment for Order ID '{savedPayment.OrderId} has been successfully saved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Saving Payment";
                response.Description = "An error occurred while saving the payment.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }
    }
}
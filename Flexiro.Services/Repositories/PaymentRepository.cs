using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;

namespace Flexiro.Services.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Payment> SavePaymentAsync(Payment paymentRequest)
        {
            try
            {
                // Map and add the new payment to the database
                var newPayment = paymentRequest;

                await _unitOfWork.Repository.AddAsync(newPayment);
                await _unitOfWork.Repository.CompleteAsync();

                // Return the saved payment entity
                return newPayment;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the payment.", ex);
            }
        }
    }
}
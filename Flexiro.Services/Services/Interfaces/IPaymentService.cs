using Flexiro.Application.Models;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ResponseModel<Payment>> SavePaymentAsync(Payment paymentRequest);
    }
}
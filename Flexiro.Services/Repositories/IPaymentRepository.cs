using Flexiro.Application.Models;

namespace Flexiro.Services.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> SavePaymentAsync(Payment paymentRequest);
    }
}

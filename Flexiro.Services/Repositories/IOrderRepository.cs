using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;

namespace Flexiro.Services.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(string userId, AddUpdateShippingAddressRequest shippingAddressDto, string paymentMethod);
        Task<int> GetTotalOrdersByShopAsync(int shopId);
        Task<(List<Order>, int)> GetDeliveredOrdersByShopAsync(int shopId);
        Task<(List<string>, int)> GetAllCustomersByShopAsync(int shopId);
        Task<decimal> GetTotalEarningsByShopAsync(int shopId);
        Task<int> GetNewOrderCountByShopAsync(int shopId);
        Task<List<OrderDetailDto>> GetOrdersByShopAsync(int shopId);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task UpdateOrderAsync(Order order);
    }
}
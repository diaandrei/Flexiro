using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseModel<OrderResponseDto>> PlaceOrderAsync(string userId, AddUpdateShippingAddressRequest shippingAddressDto, string paymentMethod);
        Task<int> GetTotalOrdersByShopAsync(int shopId);
        Task<(List<Order>, int)> GetDeliveredOrdersByShopAsync(int shopId);
        Task<(List<string>, int)> GetAllCustomersByShopAsync(int shopId);
        Task<decimal> GetTotalEarningsByShopAsync(int shopId);
        Task<int> GetNewOrderCountByShopAsync(int shopId);
        Task<GroupedOrdersDto> GetGroupedOrdersByShopAsync(int shopId);
        Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto request);
        Task<List<CustomerOrderResponseDto>> GetOrdersByCustomerAsync(string userId);
    }
}
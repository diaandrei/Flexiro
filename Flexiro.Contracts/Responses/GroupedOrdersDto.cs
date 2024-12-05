using Flexiro.Application.DTOs;

namespace Flexiro.Contracts.Responses
{
    public class GroupedOrdersDto
    {
        public List<OrderDetailDto> NewOrders { get; set; } = new();
        public List<OrderDetailDto> PendingOrders { get; set; } = new();
        public List<OrderDetailDto> ProcessingOrders { get; set; } = new();
        public List<OrderDetailDto> ShippedOrders { get; set; } = new();
        public List<OrderDetailDto> DeliveredOrders { get; set; } = new();
        public List<OrderDetailDto> CanceledOrders { get; set; } = new();
        public List<OrderDetailDto> ReturnedOrders { get; set; } = new();
        public List<OrderDetailDto> CompletedOrders { get; set; } = new();
        public List<OrderDetailDto> AllOrders { get; set; } = new();
    }
}
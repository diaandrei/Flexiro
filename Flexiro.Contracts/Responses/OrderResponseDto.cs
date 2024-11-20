using Flexiro.Application.Models;

namespace Flexiro.Contracts.Responses
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal ItemsTotal { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public ShippingAddressResponseDto ShippingAddress { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; }
    }
}
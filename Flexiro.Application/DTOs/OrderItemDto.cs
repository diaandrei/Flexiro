using Flexiro.Application.Models;

namespace Flexiro.Application.DTOs
{
    public class OrderItemDto
    {
        public int OrderDetailsId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}
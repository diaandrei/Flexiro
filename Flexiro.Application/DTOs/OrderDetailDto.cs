using Flexiro.Application.Models;

namespace Flexiro.Application.DTOs
{
    public class OrderDetailDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public decimal ItemsTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Shipping and Billing Address details
        public ShippingAddressDto ShippingAddress { get; set; }
        public BillingAddressDto BillingAddress { get; set; }

        // List of order items associated with the order
        public List<OrderItemDto> OrderItems { get; set; }
    }

}
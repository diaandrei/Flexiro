using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int ShippingAddressId { get; set; }
        public virtual ShippingAddress ShippingAddress { get; set; }
        public int BillingAddressId { get; set; }
        public virtual ShippingAddress BillingAddress { get; set; }
        public string OrderNumber { get; set; }

        [Required]
        public decimal ItemsTotal { get; set; }

        public decimal? ShippingCost { get; set; }
        public decimal? Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
        public virtual Payment Payment { get; set; }
    }
}
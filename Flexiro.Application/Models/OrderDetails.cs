using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class OrderDetails
    {
        [Key]
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal? DiscountAmount { get; set; } = 0;

        [Required]
        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; }
        public DateTime? DeliveryDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
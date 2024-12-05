using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        public int CartId { get; set; }
        public virtual Cart Cart { get; set; }

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal PricePerUnit { get; set; }

        public decimal? DiscountAmount { get; set; } = 0;

        // Total price for this item in the cart (PricePerUnit * Quantity - DiscountAmount)
        [Required]
        public decimal TotalPrice { get; set; }

        // Timestamps
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

}
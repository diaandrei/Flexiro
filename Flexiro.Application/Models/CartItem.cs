using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerUnit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
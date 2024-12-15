using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flexiro.Application.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ItemsTotal { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalDiscount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ShippingCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
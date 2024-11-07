using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class UserWishlist
    {
        [Key]
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
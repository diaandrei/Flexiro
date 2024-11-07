using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        public decimal? Rating { get; set; }
        public string? Comment { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

}
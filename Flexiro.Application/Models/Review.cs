using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flexiro.Application.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Rating { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flexiro.Application.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerItem { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Weight { get; set; }

        public string ProductCondition { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public int StockQuantity { get; set; }
        public int? TotalSold { get; set; }

        [Required]
        public string SKU { get; set; }

        public AvailabilityStatus Availability { get; set; }
        public ProductStatus Status { get; set; }
        public ProductQualityStatus QualityStatus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPercentage { get; set; }

        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public virtual List<string> Tags { get; set; }
        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; }
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        [NotMapped]
        public decimal DiscountedPrice
        {
            get
            {
                if (DiscountPercentage.HasValue && DiscountPercentage.Value > 0)
                {
                    return PricePerItem - (PricePerItem * (DiscountPercentage.Value / 100));
                }
                return PricePerItem;
            }
        }
    }
}

using Flexiro.Application.Models;

namespace Flexiro.Contracts.Responses
{
    public class ProductResponseDto
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public string? Description { get; set; }
        public decimal PricePerItem { get; set; }
        public int MinimumPurchaseQuantity { get; set; }
        public int ShopId { get; set; }
        public int CategoryId { get; set; }
        public decimal Weight { get; set; }
        public required string ProductCondition { get; set; }
        public bool ImportedItem { get; set; }
        public required List<string> ProductImageUrls { get; set; }
        public int StockQuantity { get; set; }
        public required string SKU { get; set; }
        public ProductStatus Status { get; set; }
        public AvailabilityStatus Availability { get; set; }
        public virtual required ICollection<string> Tags { get; set; }
    }
}
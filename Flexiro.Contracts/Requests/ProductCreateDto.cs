using Flexiro.Application.Models;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace Flexiro.Contracts.Requests
{
    public class ProductCreateDto
    {
        public required string Name { get; set; }
        public int CategoryId { get; set; }
        public int ShopId { get; set; }
        public decimal PricePerItem { get; set; }
        public decimal Weight { get; set; }
        public int MinimumPurchase { get; set; }
        public required string ProductCondition { get; set; }
        public int Stock { get; set; }

        [SwaggerSchema("Upload product images", Format = "binary")]
        public List<IFormFile> ProductImages { get; set; }

        public required string Description { get; set; }
        public required List<string> Tags { get; set; }
        public string SKU { get; set; }
        public ProductStatus Status { get; set; }
        public AvailabilityStatus Availability { get; set; }
    }
}
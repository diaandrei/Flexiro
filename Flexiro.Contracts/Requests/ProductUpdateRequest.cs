using Flexiro.Application.Models;
using Microsoft.AspNetCore.Http;

namespace Flexiro.Contracts.Requests
{
    public class ProductUpdateRequest
    {
        public required string Name { get; set; }
        public required string Category { get; set; }
        public decimal PricePerItem { get; set; }
        public float Weight { get; set; }
        public int MinimumPurchase { get; set; }
        public required string ProductCondition { get; set; }
        public int Stock { get; set; }
        public bool ImportedItem { get; set; }
        public required List<IFormFile> Images { get; set; }
        public required string Description { get; set; }
        public required List<IFormFile> Videos { get; set; }
        public required List<string> Tags { get; set; }
        public required string SKU { get; set; }
        public bool IsAvailableForSale { get; set; }
        public ProductStatus Status { get; set; }
    }
}
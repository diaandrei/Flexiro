using Flexiro.Application.Models;
using Microsoft.AspNetCore.Http;

namespace Flexiro.Application.DTOs
{
    public class ProductUpdateDto
    {
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal? PricePerItem { get; set; }
        public int? MinimumPurchaseQuantity { get; set; }
        public decimal? Weight { get; set; }
        public string? ProductCondition { get; set; }
        public bool? ImportedItem { get; set; }
        public List<IFormFile>? ProductImages { get; set; }
        public int? StockQuantity { get; set; }
        public ProductStatus? Status { get; set; }
        public List<string>? Tags { get; set; }
    }
}
using Microsoft.AspNetCore.Http;

namespace Flexiro.Contracts.Requests
{
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public decimal PricePerItem { get; set; }
        public decimal Weight { get; set; }
        public int MinimumPurchase { get; set; }
        public string ProductCondition { get; set; }
        public int Stock { get; set; }
        public List<IFormFile> Images { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Videos { get; set; }
        public List<string> Tags { get; set; }
        public string SKU { get; set; }
    }
}
namespace Flexiro.Application.DTOs
{
    public class ProductDto
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal PricePerItem { get; set; }
        public float Weight { get; set; }
        public int MinimumPurchase { get; set; }
        public string ProductCondition { get; set; }
        public int Stock { get; set; }
        public List<string> Images { get; set; }
        public string Description { get; set; }
        public List<string> Videos { get; set; }
        public List<string> Tags { get; set; }
        public string SKU { get; set; }
    }
}
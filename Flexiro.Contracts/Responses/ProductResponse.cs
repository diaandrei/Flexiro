namespace Flexiro.Contracts.Responses
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string Description { get; set; }
        public decimal PricePerItem { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public required string MainImage { get; set; }
        public required List<string> ImageUrls { get; set; }
        public required string CategoryName { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalSold { get; set; }
    }
}
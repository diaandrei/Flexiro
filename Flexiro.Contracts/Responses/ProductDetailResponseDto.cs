namespace Flexiro.Contracts.Responses
{
    public class ProductDetailResponseDto
    {
        public int ProductId { get; set; }
        public int ShopId { get; set; }
        public required string ProductName { get; set; }
        public required string Description { get; set; }
        public decimal PricePerItem { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public required string MainImage { get; set; }
        public required List<string> ImageUrls { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalSold { get; set; }
        public bool IsInWishlist { get; set; }
        public required List<ReviewResponseDto> Reviews { get; set; }
        public required string CategoryName { get; set; }
        public int TotalStock { get; set; }
    }
}
namespace Flexiro.Contracts.Responses
{
    public class ProductSaleResponseDto
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public string? Description { get; set; }
        public required string MainImage { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime SaleEndDate { get; set; }
        public int StockQuantity { get; set; }
        public int TotalSold { get; set; }
    }
}
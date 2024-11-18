namespace Flexiro.Contracts.Responses
{
    public class ProductSummaryResponse
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int TotalSold { get; set; }
    }
}
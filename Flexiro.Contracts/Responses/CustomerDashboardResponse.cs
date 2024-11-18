namespace Flexiro.Contracts.Responses
{
    public class CustomerDashboardResponse
    {
        public required IList<ShopSummaryResponse> Shops { get; set; }
        public required IList<ProductSaleResponseDto> SaleProducts { get; set; }
        public required List<ProductTopRatedDto> TopRatedAffordableProducts { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
    }
}
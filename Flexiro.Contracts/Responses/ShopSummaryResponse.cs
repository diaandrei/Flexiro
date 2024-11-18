namespace Flexiro.Contracts.Responses
{
    public class ShopSummaryResponse
    {
        public int ShopId { get; set; }
        public required string ShopName { get; set; }
        public required string ShopLogo { get; set; }
        public required string ShopDescription { get; set; }
    }
}
namespace Flexiro.Contracts.Responses
{
    public class ProductTopRatedDto
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string Description { get; set; }
        public decimal PricePerItem { get; set; }
        public required string ShopImage { get; set; }
    }
}
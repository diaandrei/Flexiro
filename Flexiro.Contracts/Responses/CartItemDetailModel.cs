namespace Flexiro.Contracts.Responses
{
    public class CartItemDetailModel
    {
        public int CartItemId { get; set; }
        public required string ProductName { get; set; }
        public required string MainImage { get; set; }
        public int Quantity { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
namespace Flexiro.Contracts.Responses
{
    public class CartItemSummary
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
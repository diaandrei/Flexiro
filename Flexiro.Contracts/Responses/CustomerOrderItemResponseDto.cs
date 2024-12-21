namespace Flexiro.Contracts.Responses
{
    public class CustomerOrderItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

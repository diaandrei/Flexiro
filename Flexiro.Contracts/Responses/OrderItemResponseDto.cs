namespace Flexiro.Contracts.Responses
{
    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
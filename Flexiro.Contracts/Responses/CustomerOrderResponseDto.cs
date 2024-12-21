namespace Flexiro.Contracts.Responses
{
    public class CustomerOrderResponseDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal ItemsTotal { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public ShippingAddressResponseDto ShippingAddress { get; set; }
        public List<CustomerOrderItemResponseDto> OrderItems { get; set; }
    }
}

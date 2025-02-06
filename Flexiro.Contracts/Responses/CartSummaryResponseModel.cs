namespace Flexiro.Contracts.Responses
{
    public class CartSummaryResponseModel
    {
        public List<CartItemSummary> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal Total { get; set; }
    }
}
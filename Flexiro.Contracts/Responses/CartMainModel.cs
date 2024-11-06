namespace Flexiro.Contracts.Responses
{
    public class CartMainModel
    {
        public decimal ItemTotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Discount { get; set; }
        public decimal? ShippingCharges { get; set; }
        public decimal Total { get; set; }
    }
}
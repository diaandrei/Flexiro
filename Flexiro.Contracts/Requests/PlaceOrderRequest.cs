namespace Flexiro.Contracts.Requests
{
    public class PlaceOrderRequest
    {
        public required string UserId { get; set; }
        public required AddUpdateShippingAddressRequest ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
    }
}
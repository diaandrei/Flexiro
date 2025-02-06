namespace Flexiro.Contracts.Requests
{
    public class PlaceOrderRequest
    {
        public string UserId { get; set; }
        public AddUpdateShippingAddressRequest ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
    }
}
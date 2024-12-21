namespace Flexiro.Contracts.Requests
{
    public class PaymentRequest
    {
        public string OrderId { get; set; }
        public string PaymentMethodNonce { get; set; }
    }
}

namespace Flexiro.Contracts.Requests
{
    public class MultiCartItemRequestModel
    {
        public List<CartItemRequestModel> Items { get; set; }
        public bool IsGuest { get; set; }
    }
}
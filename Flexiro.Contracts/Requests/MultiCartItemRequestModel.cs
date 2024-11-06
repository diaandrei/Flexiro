namespace Flexiro.Contracts.Requests
{
    public class MultiCartItemRequestModel
    {
        public required List<CartItemRequestModel> Items { get; set; }
    }
}
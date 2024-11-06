namespace Flexiro.Contracts.Requests
{
    public class CartItemRequestModel
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }
        public int ShopId { get; set; }
        public decimal? Price { get; set; }
    }
}
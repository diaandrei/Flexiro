using Flexiro.Contracts.Responses;

namespace Flexiro.Contracts.Requests
{
    public class MainCartModel
    {
        public int CartId { get; set; }
        public List<CartItemDetailModel> Items { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Tax { get; set; }
        public decimal? ShippingCost { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
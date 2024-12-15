namespace Flexiro.Application.DTOs
{
    public class AddToWishlistRequest
    {
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string ShopId { get; set; }
    }

}
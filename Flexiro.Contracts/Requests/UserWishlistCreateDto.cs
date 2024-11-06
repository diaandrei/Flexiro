namespace Flexiro.Contracts.Requests
{
    public class UserWishlistCreateDto
    {
        public int ProductId { get; set; }
        public int ShopId { get; set; }
        public string UserId { get; set; }
    }
}
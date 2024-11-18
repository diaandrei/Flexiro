namespace Flexiro.Contracts.Requests
{
    public class UserWishlistCreateDto
    {
        public int ProductId { get; set; }
        public int ShopId { get; set; }
        public required string UserId { get; set; }
    }
}
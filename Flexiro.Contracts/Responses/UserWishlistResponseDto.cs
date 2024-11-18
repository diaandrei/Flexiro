namespace Flexiro.Contracts.Responses
{
    public class UserWishlistResponseDto
    {
        public int ProductId { get; set; }
        public int ShopId { get; set; }
        public required string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
namespace Flexiro.Application.DTOs
{
    public class UpdateCartDto
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
        public string UserId { get; set; }
    }
}
namespace Flexiro.Application.DTOs
{
    public class SellerLoginDto
    {
        public string Token { get; set; }
        public string SellerId { get; set; }
        public string Role { get; set; }
        public int ShopId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string OwnerName { get; set; }
        public string ShopName { get; set; }
    }

}
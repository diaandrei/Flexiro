using Flexiro.Application.Models;

namespace Flexiro.Contracts.Responses
{
    public class ShopResponse
    {
        public int ShopId { get; set; }
        public required string OwnerId { get; set; }
        public required string OwnerName { get; set; }
        public required string ShopName { get; set; }
        public required string ShopDescription { get; set; }
        public required string ShopLogo { get; set; }
        public ShopAdminStatus AdminStatus { get; set; }
        public ShopSellerStatus SellerStatus { get; set; }
        public required string Slogan { get; set; }
        public required List<Product> Products { get; set; }
        public DateTime OpeningDate { get; set; }
        public required string OpeningTime { get; set; }
        public required string ClosingTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
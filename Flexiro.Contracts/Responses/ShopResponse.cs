using Flexiro.Application.Models;

namespace Flexiro.Contracts.Responses
{
    public class ShopResponse
    {
        public int ShopId { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string ShopName { get; set; }
        public string ShopDescription { get; set; }
        public string ShopLogo { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public ShopAdminStatus AdminStatus { get; set; }
        public ShopSellerStatus SellerStatus { get; set; }
        public string Slogan { get; set; }
        public List<Product> Products { get; set; }
        public DateTime OpeningDate { get; set; }
        public string OpeningTime { get; set; }
        public string ClosingTime { get; set; }
        public string OpeningDay { get; set; }
        public string ClosingDay { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
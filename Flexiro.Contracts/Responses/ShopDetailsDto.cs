using Flexiro.Application.Models;

namespace Flexiro.Contracts.Responses
{
    public class ShopDetailsDto
    {
        public int ShopId { get; set; }
        public required string ShopLogo { get; set; }
        public required string ShopName { get; set; }
        public required string OwnerName { get; set; }
        public required string ShopDescription { get; set; }

        public ShopAdminStatus AdminStatus { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalOrders { get; set; }
    }
}
using Flexiro.Application.Models;
using Microsoft.AspNetCore.Http;

namespace Flexiro.Contracts.Requests
{
    public class UpdateShopRequest
    {
        public required string ShopName { get; set; }
        public required string ShopDescription { get; set; }
        public required IFormFile ShopLogo { get; set; }
        public required string Slogan { get; set; }
        public ShopSellerStatus SellerStatus { get; set; }
        public DateTime OpeningDate { get; set; }
        public required string OpeningTime { get; set; }
        public required string ClosingTime { get; set; }
    }
}
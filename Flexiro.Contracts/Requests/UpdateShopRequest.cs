using Flexiro.Application.Models;
using Microsoft.AspNetCore.Http;

namespace Flexiro.Contracts.Requests
{
    public class UpdateShopRequest
    {
        public int ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? ShopDescription { get; set; }
        public IFormFile? ShopLogo { get; set; }
        public string? NewLogoPath { get; set; }
        public string? Slogan { get; set; }
        public ShopSellerStatus? SellerStatus { get; set; }
        public DateTime? OpeningDate { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }
    }
}
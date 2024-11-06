using Microsoft.AspNetCore.Http;

namespace Flexiro.Contracts.Requests
{
    public class RegisterSellerRequest
    {
        public required string UserName { get; set; }
        public required string OwnerName { get; set; }
        public required IFormFile ShopLogo { get; set; }
        public required string StoreName { get; set; }
        public required string Slogan { get; set; }
        public required string ContactNo { get; set; }
        public required string Country { get; set; }
        public required string City { get; set; }
        public required string ZipCode { get; set; }
        public required string StoreDescription { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateTime OpeningDate { get; set; }
        public required string OpeningTime { get; set; }
        public required string ClosingTime { get; set; }
    }
}
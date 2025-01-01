using Microsoft.AspNetCore.Identity;

namespace Flexiro.Application.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAdmin { get; set; }
        public string? ProfilePic { get; set; }
        public bool IsSeller { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Postcode { get; set; }
    }
}
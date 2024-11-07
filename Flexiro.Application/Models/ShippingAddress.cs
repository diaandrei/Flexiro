using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class ShippingAddress
    {
        [Key]
        public int ShippingAddressId { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string Note { get; set; }
        public bool AddToAddressBook { get; set; }
    }
}
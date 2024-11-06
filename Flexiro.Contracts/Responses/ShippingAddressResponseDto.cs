namespace Flexiro.Contracts.Responses
{
    public class ShippingAddressResponseDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string ZipCode { get; set; }
        public required string Country { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Note { get; set; }
    }
}
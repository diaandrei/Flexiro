namespace Flexiro.Contracts.Requests
{
    public class UpdateContactInfoRequest
    {
        public string SellerId { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }

}
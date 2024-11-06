namespace Flexiro.Contracts.Requests
{
    public class UpdateContactInfoRequest
    {
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }

}
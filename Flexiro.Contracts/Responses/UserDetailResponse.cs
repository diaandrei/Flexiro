namespace Flexiro.Contracts.Responses
{
    public class UserDetailResponse
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
    }
}
namespace Flexiro.Application.DTOs
{
    public class LoginDto
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSeller { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
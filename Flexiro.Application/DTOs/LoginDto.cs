namespace Flexiro.Application.DTOs
{
    public class LoginDTO
    {
        public string Token { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSeller { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
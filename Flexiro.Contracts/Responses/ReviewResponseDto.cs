namespace Flexiro.Contracts.Responses
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public decimal Rating { get; set; }
        public required string Comment { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public required string UserName { get; set; }
    }
}
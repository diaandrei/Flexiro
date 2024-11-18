namespace Flexiro.Contracts.Requests
{
    public class AddReviewRequestDto
    {
        public int ProductId { get; set; }
        public decimal Rating { get; set; }
        public required string Comment { get; set; }
        public required string UserId { get; set; }
    }
}
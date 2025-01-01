namespace Flexiro.Contracts.Responses
{
    public class UserRatingResponseDto
    {
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public decimal Rating { get; set; }
    }

}
namespace Flexiro.Contracts.Responses
{
    internal class WishlistProductResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public string MainImage { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

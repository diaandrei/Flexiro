namespace Flexiro.Contracts.Responses
{
    public class ProductListsDto
    {
        public List<ProductResponseDto> AllProducts { get; set; }
        public required List<ProductResponseDto> ForSaleProducts { get; set; }
        public required List<ProductResponseDto> NotForSellProducts { get; set; }
        public required List<ProductResponseDto> ForSellProducts { get; set; }
        public required List<ProductResponseDto> DraftProducts { get; set; }
    }
}
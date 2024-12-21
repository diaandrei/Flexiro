namespace Flexiro.Contracts.Responses
{
    public class ProductListsDto
    {
        public List<ProductResponseDto> ForSaleProducts { get; set; }
        public List<ProductResponseDto> NotForSaleProducts { get; set; }
        public List<ProductResponseDto> ForSellProducts { get; set; }
        public List<ProductResponseDto> DraftProducts { get; set; }
        public List<ProductResponseDto> SoldOutProducts { get; set; }
    }
}
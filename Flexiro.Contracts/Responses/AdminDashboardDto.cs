namespace Flexiro.Contracts.Responses
{
    public class AdminDashboardDto
    {
        public required IList<ShopDetailsDto> ActiveShops { get; set; }
        public required IList<ShopDetailsDto> PendingShops { get; set; }
        public required IList<ShopDetailsDto> InactiveShops { get; set; }
        public required IList<ShopDetailsDto> AllShops { get; set; }
    }
}

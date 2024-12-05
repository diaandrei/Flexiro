namespace Flexiro.Application.DTOs
{
    public class ShopStatus
    {
        public int ShopId { get; set; }
        public int? NewStatus { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }
        public string? OpeningDay { get; set; }
        public string? ClosingDay { get; set; }
    }
}
namespace Flexiro.Contracts.Requests
{
    public class ShopAdminStatusRequest
    {
        public int ShopId { get; set; }
        public int NewStatus { get; set; }
    }
}
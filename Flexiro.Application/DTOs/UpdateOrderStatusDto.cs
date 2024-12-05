namespace Flexiro.Application.DTOs
{
    public class UpdateOrderStatusDto
    {
        public int OrderId { get; set; }
        public int NewStatus { get; set; }
    }
}
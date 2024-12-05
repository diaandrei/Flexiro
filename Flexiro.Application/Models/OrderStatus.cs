namespace Flexiro.Application.Models
{
    public enum OrderStatus
    {
        New = 0,
        Pending = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Canceled = 5,
        Returned = 6,
        Completed = 7
    }
}
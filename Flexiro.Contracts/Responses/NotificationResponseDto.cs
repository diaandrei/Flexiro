namespace Flexiro.Contracts.Responses
{
    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string NotificationType { get; set; }
    }
}
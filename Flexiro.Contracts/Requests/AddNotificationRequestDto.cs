namespace Flexiro.Contracts.Requests
{
    public class AddNotificationRequestDto
    {
        public string UserId { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
    }
}
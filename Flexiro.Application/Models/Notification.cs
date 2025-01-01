using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        public string NotificationType { get; set; }
    }

}
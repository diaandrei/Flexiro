using Flexiro.Contracts.Requests;
using Flexiro.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Flexiro.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddNotification([FromBody] AddNotificationRequestDto request)
        {
            await _notificationService.AddNotificationAsync(request.UserId, request.Message, request.NotificationType);
            return Ok(new { Message = "Notification added successfully." });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(string userId)
        {
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPost("mark-as-read/{userId}")]
        public async Task<IActionResult> MarkAsRead(string userId)
        {
            await _notificationService.MarkNotificationsAsReadAsync(userId);
            return Ok(new { Message = "Notifications marked as read." });
        }
    }

}
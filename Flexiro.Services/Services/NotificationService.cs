using Flexiro.Application.Models;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Flexiro.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationRepository notificationRepository, IHubContext<NotificationHub> hubContext)
        {

            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        public async Task AddNotificationAsync(string userId, string message, string notificationType)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                NotificationType = notificationType,
                IsRead = false
            };

            await _notificationRepository.AddNotificationAsync(notification);
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task<List<NotificationResponseDto>> GetNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);

            return notifications.Select(n => new NotificationResponseDto
            {
                NotificationId = n.NotificationId,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                NotificationType = n.NotificationType
            }).ToList();
        }

        public async Task MarkNotificationsAsReadAsync(string userId)
        {
            await _notificationRepository.MarkNotificationsAsReadAsync(userId);
        }
    }
}

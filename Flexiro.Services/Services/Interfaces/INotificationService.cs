using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface INotificationService
    {
        Task AddNotificationAsync(string userId, string message, string notificationType);
        Task<List<NotificationResponseDto>> GetNotificationsAsync(string userId);
        Task MarkNotificationsAsReadAsync(string userId);
    }
}
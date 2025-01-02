using Flexiro.Application.Models;

namespace Flexiro.Services.Repositories
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
        Task MarkNotificationsAsReadAsync(string userId);
    }
}
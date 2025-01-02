using Microsoft.AspNetCore.SignalR;

namespace Flexiro.Services.Services
{
    public class NotificationHub : Hub
    {
        private readonly UserConnectionManager _userConnectionManager;

        public NotificationHub(UserConnectionManager userConnectionManager)
        {
            _userConnectionManager = userConnectionManager;
        }

        public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task OnConnected(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnectionManager.AddConnection(userId, Context.ConnectionId);
            }

            await Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _userConnectionManager.RemoveConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
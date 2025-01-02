using System.Collections.Concurrent;

namespace Flexiro.Services.Services
{
    public class UserConnectionManager
    {
        private static readonly ConcurrentDictionary<string, string> _connections = new();

        public void AddConnection(string userId, string connectionId)
        {
            _connections[userId] = connectionId;
        }

        public void RemoveConnection(string connectionId)
        {
            var item = _connections.FirstOrDefault(kvp => kvp.Value == connectionId);
            if (item.Key != null)
            {
                _connections.TryRemove(item.Key, out _);
            }
        }

        public string GetConnection(string userId)
        {
            _connections.TryGetValue(userId, out var connectionId);

            return connectionId;
        }
    }
}
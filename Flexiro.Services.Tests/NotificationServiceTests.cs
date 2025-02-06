using Flexiro.Application.Models;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Flexiro.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly NotificationService _notificationService;
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IHubClients> _hubClientsMock;

        public NotificationServiceTests()
        {
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _hubContextMock = new Mock<IHubContext<NotificationHub>>();
            _clientProxyMock = new Mock<IClientProxy>();
            _hubClientsMock = new Mock<IHubClients>();
            _hubClientsMock.Setup(clients => clients.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);
            _hubContextMock.Setup(hub => hub.Clients).Returns(_hubClientsMock.Object);
            _notificationService = new NotificationService(_notificationRepositoryMock.Object, _hubContextMock.Object);
        }

        [Fact]
        public async Task AddNotificationAsync_SendsNotification()
        {
            // Arrange
            var userId = "testUser";
            var message = "Hello";
            var notificationType = "TestType";

            // Act
            await _notificationService.AddNotificationAsync(userId, message, notificationType);

            // Assert
            _notificationRepositoryMock.Verify(repo => repo.AddNotificationAsync(It.Is<Notification>(
                n => n.UserId == userId && n.Message == message && n.NotificationType == notificationType
            )), Times.Once);
            _clientProxyMock.Verify(proxy => proxy.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), default), Times.Once);
        }

        [Fact]
        public async Task GetNotificationsAsync_ReturnsNotifications()
        {
            // Arrange
            var userId = "testUser";
            var notifications = new List<Notification>
            {
                new Notification { NotificationId = 1, UserId = userId, Message = "Msg1", NotificationType = "Type1" },
                new Notification { NotificationId = 2, UserId = userId, Message = "Msg2", NotificationType = "Type2" }
            };
            _notificationRepositoryMock.Setup(repo => repo.GetUserNotificationsAsync(userId)).ReturnsAsync(notifications);

            // Act
            var result = await _notificationService.GetNotificationsAsync(userId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Msg1", result[0].Message);
            Assert.Equal("Type1", result[0].NotificationType);
            Assert.Equal("Msg2", result[1].Message);
            Assert.Equal("Type2", result[1].NotificationType);
        }

        [Fact]
        public async Task MarkNotificationsAsReadAsync_CallsRepository()
        {
            // Arrange
            var userId = "testUser";

            // Act
            await _notificationService.MarkNotificationsAsReadAsync(userId);

            // Assert
            _notificationRepositoryMock.Verify(repo => repo.MarkNotificationsAsReadAsync(userId), Times.Once);
        }
    }
}

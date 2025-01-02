using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Flexiro.Services.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public NotificationRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _unitOfWork.Repository.AddAsync(notification);
            await _unitOfWork.Repository.CompleteAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _unitOfWork.Repository
                .GetQueryable<Notification>()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkNotificationsAsReadAsync(string userId)
        {
            var notifications = await _unitOfWork.Repository
                .GetQueryable<Notification>()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _unitOfWork.Repository.CompleteAsync();
        }
    }
}
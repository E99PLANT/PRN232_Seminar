using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Services
{
    public class NotificationsService(INotificationRepo notificationRepo) : INotificationService
    {
        private readonly INotificationRepo _notificationRepo = notificationRepo;

        public List<Notification> GetAll()
        {
            return _notificationRepo.GetAll().OrderByDescending(a => a.MessagedAt).ToList();
        }
    }
}

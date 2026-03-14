using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationService
    {
        List<Notification> GetAll();
    }
}

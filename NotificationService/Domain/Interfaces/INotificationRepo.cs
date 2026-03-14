using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces
{
    public interface INotificationRepo
    {
        Task CreateAsync(Notification notification);
        List<Notification> GetAll();
    }
}

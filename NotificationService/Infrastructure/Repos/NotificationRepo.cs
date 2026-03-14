using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repos
{
    public class NotificationRepo(NotificationDbContext context) : INotificationRepo
    {
        private readonly NotificationDbContext _context = context;

        public async Task CreateAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public List<Notification> GetAll()
        {
            return _context.Notifications.ToList();
        }
    }
}

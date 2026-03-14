using NotificationService.Domain.Entities;
using NotificationService.Domain.Events;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Application.Handlers.Impls
{
    public class NotificationHandler(NotificationDbContext dbContext) : INotificationHandler
    {
        private readonly NotificationDbContext _dbContext = dbContext;

        public async Task HandleAsync(TransactionApproved @event)
        {
            var notification = new Notification()
            {
                Id = Guid.NewGuid(),
                Message = $"Giao dịch {@event.Id} thành công",
                MessagedAt = DateTime.UtcNow.AddHours(-5),
            };

            await _dbContext.Notifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();
        }
    }
}

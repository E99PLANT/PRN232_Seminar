using NotificationService.Domain.Events;

namespace NotificationService.Application.Handlers
{
    public interface INotificationHandler
    {
        Task HandleAsync(TransactionApproved @event);
    }
}

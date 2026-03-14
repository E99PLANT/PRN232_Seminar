using BalanceService.Domain.Events;

namespace BalanceService.Application.Handlers
{
    public interface IBalanceHandler
    {
        Task HandleAsync(TransactionApproved @event);
    }
}

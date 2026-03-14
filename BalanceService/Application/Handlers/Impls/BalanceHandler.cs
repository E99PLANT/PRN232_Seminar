using BalanceService.Domain.Events;
using BalanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BalanceService.Application.Handlers.Impls
{
    public class BalanceHandler(BalanceDbContext dbContext) : IBalanceHandler
    {
        private readonly BalanceDbContext _dbContext = dbContext;

        public async Task HandleAsync(TransactionApproved @event)
        {
            var balance = await _dbContext.Balances.FirstOrDefaultAsync();
            if (balance != null)
            {
                balance.Amount += @event.Amount;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

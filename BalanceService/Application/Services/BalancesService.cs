using BalanceService.Application.DTOs;
using BalanceService.Application.Interfaces;
using BalanceService.Domain.Entities;
using BalanceService.Domain.Interfaces;

namespace BalanceService.Application.Services
{
    public class BalancesService(IBalanceRepository repo) : IBalanceService
    {
        public async Task CreateBalance(BalanceRequest req)
        {
            var balance = new UserBalance
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                Amount = req.Amount
            };
            await repo.CreateAsync(balance);
            await repo.SaveChangesAsync();
        }
        public UserBalance? GetBalance() => repo.GetFirst();
    }
}

using BalanceService.Domain.Entities;
using BalanceService.Domain.Interfaces;
using BalanceService.Infrastructure.Data;

namespace BalanceService.Infrastructure.Repos
{
    public class BalanceRepository(BalanceDbContext dbContext) : IBalanceRepository
    {
        public async Task CreateAsync(UserBalance balance) => await dbContext.Balances.AddAsync(balance);
        public UserBalance? GetFirst() => dbContext.Balances.FirstOrDefault();
        public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
    }
}

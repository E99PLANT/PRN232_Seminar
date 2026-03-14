using BalanceService.Domain.Entities;

namespace BalanceService.Domain.Interfaces
{
    public interface IBalanceRepository
    {
        Task CreateAsync(UserBalance balance);
        UserBalance? GetFirst();
        Task SaveChangesAsync();
    }
}

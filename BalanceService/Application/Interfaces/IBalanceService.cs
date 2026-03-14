using BalanceService.Application.DTOs;
using BalanceService.Domain.Entities;

namespace BalanceService.Application.Interfaces
{
    public interface IBalanceService
    {
        Task CreateBalance(BalanceRequest req);
        UserBalance? GetBalance();
    }
}

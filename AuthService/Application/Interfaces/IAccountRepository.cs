using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(string id);
        Task<Account?> GetByEmailAsync(string email);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
    }
}

using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(string id)
        {
            return await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Account?> GetByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
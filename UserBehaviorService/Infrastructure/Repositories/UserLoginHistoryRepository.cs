using Microsoft.EntityFrameworkCore;
using UserBehaviorService.Application.Interfaces;
using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Infrastructure.Repositories
{
    public class UserLoginHistoryRepository : IUserLoginHistoryRepository
    {
        private readonly AppDbContext _context;

        public UserLoginHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserLoginHistory history)
        {
            await _context.UserLoginHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserLoginHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.UserLoginHistories
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.LoggedInAt)
                .ToListAsync();
        }
    }
}

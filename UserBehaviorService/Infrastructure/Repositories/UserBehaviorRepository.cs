using Microsoft.EntityFrameworkCore;
using UserBehaviorService.Application.Interfaces;
using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Infrastructure.Repositories
{
    public class UserBehaviorRepository : IUserBehaviorRepository
    {
        private readonly AppDbContext _context;

        public UserBehaviorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserBehaviorProjection?> GetByUserIdAsync(string userId)
        {
            return await _context.UserBehaviorProjections.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(UserBehaviorProjection projection)
        {
            await _context.UserBehaviorProjections.AddAsync(projection);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserBehaviorProjection projection)
        {
            _context.UserBehaviorProjections.Update(projection);
            await _context.SaveChangesAsync();
        }
    }
}

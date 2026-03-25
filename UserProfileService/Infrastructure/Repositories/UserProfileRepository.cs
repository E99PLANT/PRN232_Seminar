using Microsoft.EntityFrameworkCore;
using UserProfileService.Application.Interfaces;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Infrastructure.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;

        public UserProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetByIdAsync(string id)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<UserProfile?> GetByAccountIdAsync(string accountId)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(x => x.AccountId == accountId);
        }

        public async Task AddAsync(UserProfile profile)
        {
            await _context.UserProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}

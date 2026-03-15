using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByIdAsync(string id);
        Task<UserProfile?> GetByAccountIdAsync(string accountId);
        Task AddAsync(UserProfile profile);
        Task UpdateAsync(UserProfile profile);
    }
}

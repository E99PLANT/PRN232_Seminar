using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Application.Interfaces
{
    public interface IUserBehaviorRepository
    {
        Task<UserBehaviorProjection?> GetByUserIdAsync(string userId);
        Task AddAsync(UserBehaviorProjection projection);
        Task UpdateAsync(UserBehaviorProjection projection);
    }
}

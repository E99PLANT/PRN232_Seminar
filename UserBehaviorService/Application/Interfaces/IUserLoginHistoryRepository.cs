using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Application.Interfaces
{
    public interface IUserLoginHistoryRepository
    {
        Task AddAsync(UserLoginHistory history);
        Task<List<UserLoginHistory>> GetByUserIdAsync(string userId);
    }
}

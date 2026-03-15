using UserBehaviorService.Application.ReadModels;

namespace UserBehaviorService.Application.Interfaces
{
    public interface IUserBehaviorAnalyticsService
    {
        Task HandleUserRegisteredAsync(string messageId, string userId, string email, DateTimeOffset occurredOn);
        Task HandleUserVerifiedAsync(string messageId, string userId, string email, DateTimeOffset occurredOn);
        Task HandleUserLoggedInAsync(string messageId, string userId, string email, DateTimeOffset occurredOn);
        Task HandleUserLockedAsync(string messageId, string userId, DateTimeOffset occurredOn);
        Task HandleUserUnlockedAsync(string messageId, string userId, DateTimeOffset occurredOn);
        Task HandleUserProfileCreatedAsync(string messageId, string userId, string email, DateTimeOffset occurredOn);
        Task HandleUserProfileUpdatedAsync(string messageId, string userId, DateTimeOffset occurredOn);

        Task<UserBehaviorReadModel?> GetByUserIdAsync(string userId);
    }
}

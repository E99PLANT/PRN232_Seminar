using UserBehaviorService.Application.ReadModels;

namespace UserBehaviorService.Application.Interfaces
{
    public interface IUserBehaviorEventReplayService
    {
        Task<UserBehaviorEventReplayDetailReadModel> GetDetailAsync(
            string userId,
            CancellationToken cancellationToken = default);
    }
}

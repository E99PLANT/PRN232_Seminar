using AuthService.Application.ReadModels;

namespace AuthService.Application.Interfaces
{
    public interface IAuthReplayService
    {
        Task<AuthBehaviorReadModel> ReplayAsync(string userId);
    }
}

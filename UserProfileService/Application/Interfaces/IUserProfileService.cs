using UserProfileService.Application.DTOs.Profile;
using UserProfileService.Application.ReadModels;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfile> CreateFromAuthAsync(CreateProfileFromAuthRequest request);
        Task<UserProfile?> GetByAccountIdAsync(string accountId);
        Task<UserProfile> UpdateAsync(string accountId, UpdateProfileRequest request);
        Task<UserProfileBehaviorReadModel> ReplayAsync(string accountId);
    }
}

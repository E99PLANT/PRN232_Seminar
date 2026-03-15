using UserProfileService.Application.DTOs.Profile;
using UserProfileService.Application.Interfaces;
using UserProfileService.Application.Projectors;
using UserProfileService.Application.ReadModels;
using UserProfileService.Domain.Entities;
using UserProfileService.Domain.Events;
using UserProfileService.Contracts;

namespace UserProfileService.Application.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IEventStoreService _eventStoreService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly UserProfileBehaviorProjector _projector = new();

        public UserProfileService(
            IUserProfileRepository userProfileRepository,
            IEventStoreService eventStoreService,
            IMessagePublisher messagePublisher)
        {
            _userProfileRepository = userProfileRepository;
            _eventStoreService = eventStoreService;
            _messagePublisher = messagePublisher;
        }

        public async Task<UserProfile> CreateFromAuthAsync(CreateProfileFromAuthRequest request)
        {
            var existed = await _userProfileRepository.GetByAccountIdAsync(request.UserId);
            if (existed != null) return existed;

            var profile = new UserProfile
            {
                AccountId = request.UserId,
                Email = request.Email
            };

            await _userProfileRepository.AddAsync(profile);

            await _eventStoreService.AppendAsync(profile.AccountId, "UserProfile", new UserProfileCreatedEvent
            {
                UserId = profile.AccountId,
                Email = profile.Email
            });

            await _messagePublisher.PublishAsync("userprofile.created", new UserProfileCreatedIntegrationEvent
            {
                UserId = profile.AccountId,
                Email = profile.Email,
                OccurredOn = DateTimeOffset.UtcNow
            });

            return profile;
        }

        public async Task<UserProfile?> GetByAccountIdAsync(string accountId)
        {
            return await _userProfileRepository.GetByAccountIdAsync(accountId);
        }

        public async Task<UserProfile> UpdateAsync(string accountId, UpdateProfileRequest request)
        {
            var profile = await _userProfileRepository.GetByAccountIdAsync(accountId)
                ?? throw new Exception("Profile not found.");

            profile.FullName = request.FullName;
            profile.Dob = request.Dob;
            profile.Gender = request.Gender;
            profile.Address = request.Address;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            await _userProfileRepository.UpdateAsync(profile);

            await _eventStoreService.AppendAsync(profile.AccountId, "UserProfile", new UserProfileUpdatedEvent
            {
                UserId = profile.AccountId,
                FullName = profile.FullName,
                Address = profile.Address,
                Gender = profile.Gender,
                Dob = profile.Dob
            });

            await _messagePublisher.PublishAsync("userprofile.updated", new UserProfileUpdatedIntegrationEvent
            {
                UserId = profile.AccountId,
                FullName = profile.FullName,
                Address = profile.Address,
                Gender = profile.Gender,
                Dob = profile.Dob,
                OccurredOn = DateTimeOffset.UtcNow
            });

            return profile;
        }

        public async Task<UserProfileBehaviorReadModel> ReplayAsync(string accountId)
        {
            var events = await _eventStoreService.GetEventsAsync(accountId);
            return _projector.Build(accountId, events);
        }
    }
}

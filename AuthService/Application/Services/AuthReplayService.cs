using AuthService.Application.Interfaces;
using AuthService.Application.Projectors;
using AuthService.Application.ReadModels;

namespace AuthService.Application.Services
{
    public class AuthReplayService : IAuthReplayService
    {
        private readonly IEventStoreService _eventStoreService;
        private readonly AuthBehaviorProjector _projector = new();

        public AuthReplayService(IEventStoreService eventStoreService)
        {
            _eventStoreService = eventStoreService;
        }

        public async Task<AuthBehaviorReadModel> ReplayAsync(string userId)
        {
            var events = await _eventStoreService.GetEventsAsync(userId);
            return _projector.Build(userId, events);
        }
    }
}

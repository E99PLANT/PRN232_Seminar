using UserBehaviorService.Application.MessagingContracts;

namespace UserBehaviorService.Application.ReadModels
{
    public class UserBehaviorEventReplayDetailReadModel
    {
        public string UserId { get; set; } = default!;
        public List<EventItemMessage> AuthEvents { get; set; } = new();
        public List<EventItemMessage> ProfileEvents { get; set; } = new();
        public UserBehaviorEventReplaySummaryReadModel Summary { get; set; } = new();
    }
}

namespace UserBehaviorService.Application.MessagingContracts
{
    public class UserProfileReplayRequestMessage
    {
        public string UserId { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
    }
}

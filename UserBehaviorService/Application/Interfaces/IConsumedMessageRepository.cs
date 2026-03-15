namespace UserBehaviorService.Application.Interfaces
{
    public interface IConsumedMessageRepository
    {
        Task<bool> ExistsAsync(string messageId);
        Task AddAsync(string messageId, string eventType);
    }
}

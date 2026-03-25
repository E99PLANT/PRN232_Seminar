using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Interfaces
{
    public interface IEventStoreRepository
    {
        Task AddAsync(EventStoreRecord record);
        Task<int> CountByAggregateIdAsync(string aggregateId);
        Task<List<EventStoreRecord>> GetByAggregateIdAsync(string aggregateId);
    }
}

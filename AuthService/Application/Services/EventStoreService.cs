using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Events;
using System.Text.Json;

namespace AuthService.Application.Services
{
    public class EventStoreService : IEventStoreService
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly JsonSerializerOptions _jsonOptions;

        public EventStoreService(IEventStoreRepository eventStoreRepository)
        {
            _eventStoreRepository = eventStoreRepository;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task AppendAsync(string aggregateId, string aggregateType, DomainEvent @event)
        {
            var currentCount = await _eventStoreRepository.CountByAggregateIdAsync(aggregateId);

            var record = new EventStoreRecord
            {
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                EventType = @event.GetType().Name,
                EventData = JsonSerializer.Serialize(@event, @event.GetType(), _jsonOptions),
                OccurredOn = @event.OccurredOn,
                Version = currentCount + 1
            };

            await _eventStoreRepository.AddAsync(record);
        }

        public async Task<List<DomainEvent>> GetEventsAsync(string aggregateId)
        {
            var records = await _eventStoreRepository.GetByAggregateIdAsync(aggregateId);
            var result = new List<DomainEvent>();

            foreach (var record in records)
            {
                DomainEvent? domainEvent = record.EventType switch
                {
                    nameof(UserRegisteredEvent) =>
                        JsonSerializer.Deserialize<UserRegisteredEvent>(record.EventData, _jsonOptions),

                    nameof(UserOtpVerifiedEvent) =>
                        JsonSerializer.Deserialize<UserOtpVerifiedEvent>(record.EventData, _jsonOptions),

                    nameof(UserLoggedInEvent) =>
                        JsonSerializer.Deserialize<UserLoggedInEvent>(record.EventData, _jsonOptions),

                    nameof(UserLockedEvent) =>
                        JsonSerializer.Deserialize<UserLockedEvent>(record.EventData, _jsonOptions),

                    nameof(UserUnlockedEvent) =>
                        JsonSerializer.Deserialize<UserUnlockedEvent>(record.EventData, _jsonOptions),

                    _ => null
                };

                if (domainEvent != null)
                    result.Add(domainEvent);
            }

            return result;
        }
    }
}

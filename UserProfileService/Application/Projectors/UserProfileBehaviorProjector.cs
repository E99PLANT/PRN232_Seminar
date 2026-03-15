using UserProfileService.Application.ReadModels;
using UserProfileService.Domain.Events;

namespace UserProfileService.Application.Projectors
{
    public class UserProfileBehaviorProjector
    {
        public UserProfileBehaviorReadModel Build(string userId, IEnumerable<DomainEvent> events)
        {
            var model = new UserProfileBehaviorReadModel
            {
                UserId = userId
            };

            foreach (var ev in events.OrderBy(x => x.OccurredOn))
            {
                switch (ev)
                {
                    case UserProfileCreatedEvent e:
                        model.Email = e.Email;
                        break;

                    case UserProfileUpdatedEvent:
                        model.ProfileUpdateCount++;
                        model.LastProfileUpdatedAt = ev.OccurredOn;
                        break;
                }
            }

            return model;
        }
    }
}

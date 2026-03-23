using Microsoft.AspNetCore.Mvc;
using UserProfileService.Application.Interfaces;

namespace UserProfileService.Controllers
{
    [ApiController]
    [Route("api/internal/profile-events")]
    public class InternalProfileEventsController : ControllerBase
    {
        private readonly IEventStoreService _eventStoreService;
        private readonly IUserProfileService _userProfileService;

        public InternalProfileEventsController(
            IEventStoreService eventStoreService,
            IUserProfileService userProfileService)
        {
            _eventStoreService = eventStoreService;
            _userProfileService = userProfileService;
        }

        [HttpGet("{userId}/raw")]
        public async Task<IActionResult> GetRawEvents(string userId)
        {
            var events = await _eventStoreService.GetEventsAsync(userId);

            var data = events
                .OrderBy(x => x.OccurredOn)
                .Select(x => new
                {
                    eventType = x.GetType().Name,
                    occurredOn = x.OccurredOn,
                    payload = x
                })
                .ToList();

            return Ok(new
            {
                status = 200,
                msg = "Raw profile events fetched successfully.",
                data
            });
        }

        [HttpGet("{userId}/replay")]
        public async Task<IActionResult> Replay(string userId)
        {
            var data = await _userProfileService.ReplayAsync(userId);

            return Ok(new
            {
                status = 200,
                msg = "Replay successfully.",
                data
            });
        }
    }
}

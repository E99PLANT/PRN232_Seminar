using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/internal/auth-events")]
    public class InternalAuthEventsController : ControllerBase
    {
        private readonly IEventStoreService _eventStoreService;

        public InternalAuthEventsController(IEventStoreService eventStoreService)
        {
            _eventStoreService = eventStoreService;
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
                msg = "Raw auth events fetched successfully.",
                data
            });
        }
    }
}

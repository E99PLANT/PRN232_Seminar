using Microsoft.AspNetCore.Mvc;
using UserBehaviorService.Application.Interfaces;

namespace UserBehaviorService.Controllers
{
    [ApiController]
    [Route("api/user-behaviors/event-replay")]
    public class UserBehaviorEventReplayController : ControllerBase
    {
        private readonly IUserBehaviorEventReplayService _service;

        public UserBehaviorEventReplayController(IUserBehaviorEventReplayService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId, CancellationToken cancellationToken)
        {
            var data = await _service.GetDetailAsync(userId, cancellationToken);

            return Ok(new
            {
                status = 200,
                msg = "Success",
                data
            });
        }
    }
}

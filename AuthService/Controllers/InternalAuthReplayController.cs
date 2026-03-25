using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/internal/auth-events")]
    public class InternalAuthReplayController : ControllerBase
    {
        private readonly IAuthReplayService _authReplayService;

        public InternalAuthReplayController(IAuthReplayService authReplayService)
        {
            _authReplayService = authReplayService;
        }

        [HttpGet("{userId}/replay")]
        public async Task<IActionResult> Replay(string userId)
        {
            var data = await _authReplayService.ReplayAsync(userId);

            return Ok(new
            {
                status = 200,
                msg = "Replay successfully.",
                data
            });
        }
    }
}

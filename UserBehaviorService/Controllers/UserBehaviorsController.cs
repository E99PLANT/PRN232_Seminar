using Microsoft.AspNetCore.Mvc;
using UserBehaviorService.Application.Interfaces;

namespace UserBehaviorService.Controllers
{
    [ApiController]
    [Route("api/user-behaviors")]
    public class UserBehaviorsController : ControllerBase
    {
        private readonly IUserBehaviorAnalyticsService _analyticsService;

        public UserBehaviorsController(IUserBehaviorAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var data = await _analyticsService.GetByUserIdAsync(userId);
            if (data == null) return NotFound(new { msg = "User behavior not found." });

            return Ok(new
            {
                status = 200,
                msg = "Success",
                data
            });
        }
    }
}

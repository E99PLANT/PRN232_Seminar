using Microsoft.AspNetCore.Mvc;
using UserProfileService.Application.DTOs.Common;
using UserProfileService.Application.DTOs.Profile;
using UserProfileService.Application.Interfaces;

namespace UserProfileService.Controllers
{
    [ApiController]
    [Route("api/profiles")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _service;

        public UserProfilesController(IUserProfileService service)
        {
            _service = service;
        }

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetByAccountId(string accountId)
        {
            var data = await _service.GetByAccountIdAsync(accountId);
            return Ok(ApiResponse<object>.Success(data, "Get profile successfully."));
        }

        [HttpPut("{accountId}")]
        public async Task<IActionResult> Update(string accountId, [FromBody] UpdateProfileRequest request)
        {
            var data = await _service.UpdateAsync(accountId, request);
            return Ok(ApiResponse<object>.Success(data, "Profile updated successfully."));
        }

        [HttpGet("{accountId}/replay")]
        public async Task<IActionResult> Replay(string accountId)
        {
            var data = await _service.ReplayAsync(accountId);
            return Ok(ApiResponse<object>.Success(data, "Replay successfully."));
        }
    }
}

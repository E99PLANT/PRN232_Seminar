using Microsoft.AspNetCore.Mvc;
using UserProfileService.Application.DTOs.Common;
using UserProfileService.Application.DTOs.Profile;
using UserProfileService.Application.Interfaces;

namespace UserProfileService.Controllers
{
    [ApiController]
    [Route("api/internal/profiles")]
    public class InternalProfilesController : ControllerBase
    {
        private readonly IUserProfileService _service;

        public InternalProfilesController(IUserProfileService service)
        {
            _service = service;
        }

        [HttpPost("create-from-auth")]
        public async Task<IActionResult> CreateFromAuth([FromBody] CreateProfileFromAuthRequest request)
        {
            var data = await _service.CreateFromAuthAsync(request);
            return Ok(ApiResponse<object>.Success(data, "Profile created from auth."));
        }
    }
}

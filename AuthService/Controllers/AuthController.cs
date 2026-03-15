using AuthService.Application.DTOs.Auth;
using AuthService.Application.DTOs.Common;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register/request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] RegisterRequest request)
        {
            await _service.RequestRegisterOtpAsync(request);
            return Ok(ApiResponse<object>.Success(null, "OTP sent successfully."));
        }

        [HttpPost("register/verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var token = await _service.VerifyRegisterOtpAsync(request);
            return Ok(ApiResponse<object>.Success(new { token }, "Verify successfully."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _service.LoginAsync(request);
            return Ok(ApiResponse<object>.Success(new { token }, "Login successfully."));
        }

        [HttpPut("{userId}/lock")]
        public async Task<IActionResult> Lock(string userId, [FromBody] LockAccountRequest request)
        {
            await _service.LockAsync(userId, request.Reason);
            return Ok(ApiResponse<object>.Success(null, "Locked successfully."));
        }

        [HttpPut("{userId}/unlock")]
        public async Task<IActionResult> Unlock(string userId)
        {
            await _service.UnlockAsync(userId);
            return Ok(ApiResponse<object>.Success(null, "Unlocked successfully."));
        }
    }
}

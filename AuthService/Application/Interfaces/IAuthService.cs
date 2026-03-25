using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task RequestRegisterOtpAsync(RegisterRequest request);
        Task<object> VerifyRegisterOtpAsync(VerifyOtpRequest request);
        Task<object> LoginAsync(LoginRequest request);
        Task LockAsync(string userId, string reason);
        Task UnlockAsync(string userId);
    }
}

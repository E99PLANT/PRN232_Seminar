namespace AuthService.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendOtpAsync(string toEmail, string otp);
    }
}

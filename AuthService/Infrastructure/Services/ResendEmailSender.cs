using AuthService.Application.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AuthService.Infrastructure.Services
{
    public class ResendEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ResendEmailSender(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var apiKey = _configuration["Resend:ApiKey"];
            var fromEmail = _configuration["Resend:From"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Resend ApiKey is missing.");

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new Exception("Resend FromEmail is missing.");

            var payload = new
            {
                from = fromEmail,
                to = new[] { toEmail },
                subject = "Your OTP Code",
                html = $@"
                    <div style='font-family: Arial, sans-serif; line-height: 1.6'>
                        <h2>OTP Verification</h2>
                        <p>Your OTP code is:</p>
                        <h1 style='letter-spacing: 4px'>{otp}</h1>
                        <p>This OTP will expire in 5 minutes.</p>
                    </div>"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Send OTP failed. Resend response: {content}");
        }
    }
}

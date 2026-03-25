using AuthService.Application.Interfaces;

namespace AuthService.Infrastructure.Services
{
    public class ProfileProvisionClient : IProfileProvisionClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ProfileProvisionClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task CreateProfileAsync(string userId, string email)
        {
            var baseUrl = _configuration["Services:UserProfileServiceBaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("UserProfileServiceBaseUrl is missing.");

            var url = $"{baseUrl}/api/internal/profiles/create-from-auth";

            var payload = new
            {
                userId,
                email
            };

            var response = await _httpClient.PostAsJsonAsync(url, payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Create profile failed. Response: {content}");
        }
    }
}

namespace AuthService.Application.Interfaces
{
    public interface IProfileProvisionClient
    {
        Task CreateProfileAsync(string userId, string email);
    }
}

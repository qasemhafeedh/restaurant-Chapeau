

using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<User> GetUserByUsernameAsync(string username);

        // ✅ Add this method
        Task<User?> AuthenticateAsync(string username, string password);
    }
}

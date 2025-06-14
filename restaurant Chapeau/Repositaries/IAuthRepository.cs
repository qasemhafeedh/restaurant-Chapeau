using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositories
{
    public interface IAuthRepository
    {
        Task<bool> ValidateUserAsync(string username, string password, string role);
        Task<User> GetUserByUsernameAsync(string username);
    }
}


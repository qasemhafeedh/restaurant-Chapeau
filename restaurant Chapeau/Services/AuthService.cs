using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using restaurant_Chapeau.Repositories;

namespace restaurant_Chapeau.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            // Delegates to repository — role can be null
            return await _authRepository.ValidateUserAsync(username, password);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _authRepository.GetUserByUsernameAsync(username);
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var isValid = await ValidateCredentialsAsync(username, password);
            if (!isValid)
                return null;

            return await GetUserByUsernameAsync(username);
        }
    }
}

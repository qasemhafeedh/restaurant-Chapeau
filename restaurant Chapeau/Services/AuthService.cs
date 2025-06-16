
using System.Data.SqlClient;
using restaurant_Chapeau.Enums;
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

            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM Users WHERE Username = @username", conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserID = (int)reader["UserID"],
                    Username = reader["Username"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(), // ✅ Fix here
                    Role = Enum.Parse<Role>(reader["Role"].ToString()),
                };
            }

            return null;
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

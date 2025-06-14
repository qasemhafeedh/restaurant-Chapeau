using System.Data.SqlClient;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;

        public AuthService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password, string role)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Username = @username AND PasswordHash = @password AND Role = @role", conn); // ✅ Fix here
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@role", role);

            int count = (int)await cmd.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
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
                    Role = reader["Role"].ToString()
                };
            }

            return null;
        }

        public async Task<User?> AuthenticateAsync(string username, string password, string role)
        {
            bool isValid = await ValidateCredentialsAsync(username, password, role);
            if (!isValid) return null;

            return await GetUserByUsernameAsync(username);
        }
    }
}

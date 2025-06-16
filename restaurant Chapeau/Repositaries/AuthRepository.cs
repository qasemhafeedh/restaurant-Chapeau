using System.Data.SqlClient;
using restaurant_Chapeau.Enums;
using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> ValidateUserAsync(string username, string password, string role)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM Users 
                WHERE Username = @username AND Password = @password AND Role = @role", conn);

            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@role", role);

            int result = (int)await cmd.ExecuteScalarAsync();
            return result > 0;
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
                    Role = Enum.Parse<Role>(reader["Role"].ToString())
                };
            }

            return null;
        }
    }
}


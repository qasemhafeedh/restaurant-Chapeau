
using global::restaurant_Chapeau.Models;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace restaurant_Chapeau.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand
                    ("SELECT UserID, Username, PasswordHash, Role, FullName, IsActive FROM Users", conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    User user = new User
                    {
                        UserID = (int)reader["UserID"],
                        Username = reader["Username"].ToString(),
                        PasswordHash = reader["PasswordHash"].ToString(),
                        Role = reader["Role"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        IsActive = reader["IsActive"] != DBNull.Value && (bool)reader["IsActive"]
                    };
                    users.Add(user);
                }
            }

            return users;
        }


        public User GetUserById(int id)
        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM Users WHERE UserID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserID = (int)reader["UserID"],
                    Username = reader["Username"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(),
                    Role = reader["Role"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    IsActive = (bool)reader["IsActive"]
                };
            }

            return null;
        }

        public void AddUser(User user)
        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("INSERT INTO Users (Username, PasswordHash, Role, FullName, IsActive) VALUES (@u, @p, @r, @f, 1)", conn);
            cmd.Parameters.AddWithValue("@u", user.Username);
            cmd.Parameters.AddWithValue("@p", user.PasswordHash);
            cmd.Parameters.AddWithValue("@r", user.Role);
            cmd.Parameters.AddWithValue("@f", user.FullName);

            cmd.ExecuteNonQuery();
        }

        public void UpdateUser(User user)
        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("UPDATE Users SET Username = @u, PasswordHash = @p, Role = @r, FullName = @f WHERE UserID = @id", conn);
            cmd.Parameters.AddWithValue("@u", user.Username);
            cmd.Parameters.AddWithValue("@p", user.PasswordHash);
            cmd.Parameters.AddWithValue("@r", user.Role);
            cmd.Parameters.AddWithValue("@f", user.FullName);
            cmd.Parameters.AddWithValue("@id", user.UserID);

            cmd.ExecuteNonQuery();
        }

        public void ToggleActive(int id)
        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("UPDATE Users SET IsActive = 1 - IsActive WHERE UserID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
    }
}


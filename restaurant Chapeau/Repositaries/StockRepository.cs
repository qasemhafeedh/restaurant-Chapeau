using System.Data.SqlClient;
using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly string _connectionString;

        public StockRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<MenuItem> GetAllItems()
        {
            var items = new List<MenuItem>();

            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM MenuItems", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new MenuItem
                {
                    MenuItemID = (int)reader["MenuItemID"],
                    Name = reader["Name"].ToString(),
                    Category = reader["Category"].ToString(),
                    Price = (decimal)reader["Price"],
                    IsAlcoholic = (bool)reader["IsAlcoholic"],
                    VATRate = (decimal)reader["VATRate"],
                    QuantityAvailable = (int)reader["QuantityAvailable"],
                    MenuType = reader["MenuType"].ToString(),
                    RoutingTarget = reader["RoutingTarget"].ToString()
                });
            }

            return items;
        }

        public MenuItem GetItemById(int id)
        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM MenuItems WHERE MenuItemID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new MenuItem
                {
                    MenuItemID = (int)reader["MenuItemID"],
                    Name = reader["Name"].ToString(),
                    Category = reader["Category"].ToString(),
                    Price = (decimal)reader["Price"],
                    IsAlcoholic = (bool)reader["IsAlcoholic"],
                    VATRate = (decimal)reader["VATRate"],
                    QuantityAvailable = (int)reader["QuantityAvailable"],
                    MenuType = reader["MenuType"].ToString(),
                    RoutingTarget = reader["RoutingTarget"].ToString()
                };
            }

            return null;
        }

        public void UpdateItem(MenuItem item)
        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("UPDATE MenuItems SET QuantityAvailable = @quantity WHERE MenuItemID = @id", conn);
            cmd.Parameters.AddWithValue("@quantity", item.QuantityAvailable);
            cmd.Parameters.AddWithValue("@id", item.MenuItemID);

            cmd.ExecuteNonQuery();
        }
    }
}
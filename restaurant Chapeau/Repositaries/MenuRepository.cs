using restaurant_Chapeau.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using restaurant_Chapeau.Repositaries;

namespace restaurant_Chapeau.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;

        public MenuRepository(IConfiguration config)
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
        public void AddItem(MenuItem item)
        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
                INSERT INTO MenuItems (Name, Category, Price, IsAlcoholic, VATRate, QuantityAvailable, MenuType, RoutingTarget, IsActive)
                VALUES (@name, @category, @price, 0, 9.0, @qty, @menuType, @routing, 1)", conn);

            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@category", item.Category);
            cmd.Parameters.AddWithValue("@price", item.Price);
            cmd.Parameters.AddWithValue("@qty", item.QuantityAvailable);
            cmd.Parameters.AddWithValue("@menuType", item.MenuType);
            cmd.Parameters.AddWithValue("@routing", item.RoutingTarget);

            cmd.ExecuteNonQuery();
        }
    }
}
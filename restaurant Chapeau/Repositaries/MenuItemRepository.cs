using System.Data.SqlClient;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Enums;

namespace restaurant_Chapeau.Repositories
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly string _connectionString;

        public MenuItemRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<MenuItem>> GetAllAsync()
        {
            var menuItems = new List<MenuItem>();
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var query = @"
                SELECT MenuItemID, Name, Category, Price, IsAlcoholic, VATRate, 
                       QuantityAvailable, MenuType, RoutingTarget
                FROM MenuItems";

            var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                menuItems.Add(MapToMenuItem(reader));
            }

            return menuItems;
        }

        private MenuItem MapToMenuItem(SqlDataReader reader)
        {
            return new MenuItem
            {
                MenuItemID = (int)reader["MenuItemID"],
                Name = reader["Name"].ToString(),
                Category = Enum.Parse<Category>(reader["Category"].ToString()),
                Price = (decimal)reader["Price"],
                IsAlcoholic = (bool)reader["IsAlcoholic"],
                VATRate = (decimal)reader["VATRate"],
                QuantityAvailable = (int)reader["QuantityAvailable"],
                MenuType = Enum.Parse<MenuType>(reader["MenuType"].ToString()),
                RoutingTarget = Enum.Parse<RoutingTarget>(reader["RoutingTarget"].ToString())
            };
        }
        public async Task<bool> IsStockAvailableAsync(int menuItemId, int quantity)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT QuantityAvailable 
                FROM MenuItems 
                WHERE MenuItemID = @id", conn);

            cmd.Parameters.AddWithValue("@id", menuItemId);

            int available = (int)await cmd.ExecuteScalarAsync();
            return available >= quantity;
        }

        public async Task DecreaseStockAsync(int menuItemId, int quantity)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                UPDATE MenuItems 
                SET QuantityAvailable = QuantityAvailable - @qty 
                WHERE MenuItemID = @id", conn);

            cmd.Parameters.AddWithValue("@qty", quantity);
            cmd.Parameters.AddWithValue("@id", menuItemId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<MenuItem?> GetByIdAsync(int id)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
        SELECT MenuItemID, Name, Category, Price, IsAlcoholic, VATRate, 
               QuantityAvailable, MenuType, RoutingTarget 
        FROM MenuItems 
        WHERE MenuItemID = @id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToMenuItem(reader);
            }

            return null;
        }

    }
}

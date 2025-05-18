using System.Data.SqlClient;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;

namespace restaurant_Chapeau.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<MenuItem>> GetMenuItemsAsync()
        {
            var list = new List<MenuItem>();
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM MenuItems", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MenuItem
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

            return list;
        }

        public async Task<List<RestaurantTable>> GetTablesAsync()
        {
            var tables = new List<RestaurantTable>();
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cleanupCmd = new SqlCommand(@"
                UPDATE RestaurantTables 
                SET ReservationStart = NULL, ReservationEnd = NULL 
                WHERE ReservationEnd < GETDATE()", conn);
            await cleanupCmd.ExecuteNonQueryAsync();

            var cmd = new SqlCommand("SELECT * FROM RestaurantTables", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(new RestaurantTable
                {
                    TableID = (int)reader["TableID"],
                    TableNumber = (int)reader["TableNumber"],
                    ReservationStart = reader["ReservationStart"] != DBNull.Value ? (DateTime?)reader["ReservationStart"] : null,
                    ReservationEnd = reader["ReservationEnd"] != DBNull.Value ? (DateTime?)reader["ReservationEnd"] : null
                });
            }

            return tables;
        }

        public async Task<bool> IsTableReservedAsync(int tableId)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT ReservationStart, ReservationEnd FROM RestaurantTables WHERE TableID = @id", conn);
            cmd.Parameters.AddWithValue("@id", tableId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var start = reader["ReservationStart"] as DateTime?;
                var end = reader["ReservationEnd"] as DateTime?;
                return start.HasValue && end.HasValue && DateTime.Now >= start && DateTime.Now <= end;
            }

            return false;
        }

        public async Task<bool> IsStockAvailableAsync(int menuItemId, int quantity)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT QuantityAvailable FROM MenuItems WHERE MenuItemID = @id", conn);
            cmd.Parameters.AddWithValue("@id", menuItemId);

            int available = (int)await cmd.ExecuteScalarAsync();
            return available >= quantity;
        }

        public async Task DecreaseStockAsync(int menuItemId, int quantity)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("UPDATE MenuItems SET QuantityAvailable = QuantityAvailable - @qty WHERE MenuItemID = @id", conn);
            cmd.Parameters.AddWithValue("@qty", quantity);
            cmd.Parameters.AddWithValue("@id", menuItemId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> CreateOrderAsync(int tableId, string comment)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                INSERT INTO Orders (TableID, Comment, CreatedAt) 
                OUTPUT INSERTED.OrderID 
                VALUES (@table, @comment, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@table", tableId);
            cmd.Parameters.AddWithValue("@comment", comment ?? "");

            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task AddOrderItemsAsync(int orderId, List<CartItem> items)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            foreach (var item in items)
            {
                var cmd = new SqlCommand(@"
                    INSERT INTO OrderItems (OrderID, MenuItemID, Quantity, Note, RoutingTarget) 
                    VALUES (@order, @item, @qty, @note, @route)", conn);

                cmd.Parameters.AddWithValue("@order", orderId);
                cmd.Parameters.AddWithValue("@item", item.MenuItemID);
                cmd.Parameters.AddWithValue("@qty", item.Quantity);
                cmd.Parameters.AddWithValue("@note", item.Note ?? "");
                cmd.Parameters.AddWithValue("@route", item.RoutingTarget ?? "");

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task CreateInvoiceAsync(int orderId, int userId, decimal total, decimal tip, decimal vat)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                INSERT INTO Invoices (InvoiceNumber, OrderID, UserID, TotalAmount, TipAmount, VATAmount) 
                VALUES (@num, @order, @user, @total, @tip, @vat)", conn);

            cmd.Parameters.AddWithValue("@num", $"INV-{Guid.NewGuid().ToString().Substring(0, 8)}");
            cmd.Parameters.AddWithValue("@order", orderId);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.Parameters.AddWithValue("@total", total);
            cmd.Parameters.AddWithValue("@tip", tip);
            cmd.Parameters.AddWithValue("@vat", vat);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ReserveTableAsync(int tableId)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("UPDATE RestaurantTables SET ReservationStart = GETDATE(), ReservationEnd = DATEADD(MINUTE, 60, GETDATE()) WHERE TableID = @id", conn);
            cmd.Parameters.AddWithValue("@id", tableId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> SubmitOrderAsync(OrderSubmission model, int userId)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            if (await IsTableReservedAsync(model.TableID))
                return false;

            foreach (var item in model.CartItems)
            {
                if (!await IsStockAvailableAsync(item.MenuItemID, item.Quantity))
                    throw new Exception($"Insufficient stock for {item.Name}");

                await DecreaseStockAsync(item.MenuItemID, item.Quantity);
            }

            await ReserveTableAsync(model.TableID);
            int orderId = await CreateOrderAsync(model.TableID, model.Comment);
            await AddOrderItemsAsync(orderId, model.CartItems);

            decimal total = model.CartItems.Sum(i => i.Price * i.Quantity);
            decimal vat = model.CartItems.Sum(i => i.Price * i.Quantity * i.VATRate);

            await CreateInvoiceAsync(orderId, userId, total, model.TipAmount, vat);

            return true;
        }
    }
}

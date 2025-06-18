
using System.Data.SqlClient;
using restaurant_Chapeau.Enums;
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

        public async Task<int> CreateOrderAsync(OrderSubmission model, int userId)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                INSERT INTO Orders (TableID, UserID, OrderTime, TipAmount, Comment)
                OUTPUT INSERTED.OrderID
                VALUES (@table, @user, GETDATE(), @tip, @comment)", conn);

            cmd.Parameters.AddWithValue("@table", model.TableID);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.Parameters.AddWithValue("@tip", model.TipAmount);
            cmd.Parameters.AddWithValue("@comment", model.Comment ?? "");

            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task AddOrderItemsAsync(int orderId, List<CartItem> items)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            foreach (var item in items)
            {
                var cmd = new SqlCommand(@"
                    INSERT INTO OrderItems (OrderID, MenuItemID, Quantity, Status, Timestamp) 
                    VALUES (@order, @item, @qty, @status, GETDATE())", conn);

                cmd.Parameters.AddWithValue("@order", orderId);
                cmd.Parameters.AddWithValue("@item", item.MenuItemID);
                cmd.Parameters.AddWithValue("@qty", item.Quantity);
                cmd.Parameters.AddWithValue("@status", "Pending");

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}


using System.Data.SqlClient;
using restaurant_Chapeau.Enums;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using static restaurant_Chapeau.Models.Order;

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
        /////////////////////////////////////////////////////////(below this is for the bar and Kitcehn )//////////////////////////
        
        private Order ReadOrder(SqlDataReader reader)
        {
            int orderId = (int)reader["OrderId"];
            int tableNumber = (int)reader["TableID"];
            DateTime orderTime = (DateTime)reader["OrderTime"];
            string? comment = reader["Comment"] == DBNull.Value ? null : (string)reader["Comment"];

            // Build a single OrderItems object
            var item = new Order.OrderItems
            {
                Id = (int)reader["OrderItemID"],
                Name = (string)reader["MenuItemName"],
                Note = reader["Note"] == DBNull.Value ? null : (string)reader["Note"],
                courseType = Enum.TryParse<CourseType>((string)reader["Category"], out var course) ? course : CourseType.Main,
                itemStatus = Enum.TryParse<ItemStatus>((string)reader["Status"], out var itemStatus) ? itemStatus : ItemStatus.Pending,
                target = Enum.TryParse<Order.RoutingTarget>((string)reader["RoutingTarget"], out var target) ? target : Order.RoutingTarget.Kitchen
            };

            // Create a list with just that item
            List<OrderItems> items = new List<OrderItems> { item };

            // Determine overall order status based on item(s)
            OrderStatus orderStatus = GetOrderStatusFromItems(items);

            return new Order(orderId, tableNumber, orderTime, items, comment, orderStatus);
        }

        private OrderStatus GetOrderStatusFromItems(List<OrderItems> items)
        {
            foreach (var item in items)
            {
                if (item.itemStatus != ItemStatus.Ready)
                {
                    return OrderStatus.Running;
                }
            }
            return OrderStatus.Finished;
        }

        public List<Order> GetAllOrders()
        {
            var allOrders = new List<Order>();
            using (SqlConnection conn = new(_connectionString))
            {
                string query = @"SELECT o.OrderId, o.TableID, o.Comment, o.OrderTime, oi.Note,
                mi.Name AS MenuItemName, mi.Category, oi.Status, oi.OrderItemID, mi.RoutingTarget
                FROM Orders o
                JOIN OrderItems oi ON o.OrderId = oi.OrderId
                JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
                ORDER BY o.OrderTime";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Order order = ReadOrder(reader);
                    allOrders.Add(order);
                }
                reader.Close();

            }
            ;
            return allOrders;

        }

        public void UpdateOrderItemStatus(int orderItemId, ItemStatus newStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE OrderItems SET Status = @Status WHERE OrderItemID = @OrderItemID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus.ToString());
                    cmd.Parameters.AddWithValue("@OrderItemID", orderItemId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

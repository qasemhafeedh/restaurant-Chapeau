
using System;
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


        //CreateOrderAsync(...) inserts a single row in the Orders table — it gives you back orderId.
        public async Task<int> CreateOrderAsync(OrderSubmission model, int userId)
        {
            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                INSERT INTO Orders (TableID, UserID, OrderTime)
                OUTPUT INSERTED.OrderID
                VALUES (@table, @user, GETDATE())", conn);

            cmd.Parameters.AddWithValue("@table", model.TableID);
            cmd.Parameters.AddWithValue("@user", userId);


            return (int)await cmd.ExecuteScalarAsync();
        }


        // AddOrderItemsAsync(...) inserts the actual cart items(linked by that orderId) into the OrderItems this help Kitchen and bar
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
            int id = (int)reader["OrderId"];
            int table = (int)reader["TableID"];
            DateTime time = (DateTime)reader["OrderTime"];
            string? comment = reader["Comment"] == DBNull.Value ? null : (string)reader["Comment"];
            string statusStr = (string)reader["OrderStatus"];
            Enum.TryParse(statusStr, out OrderStatus status);

            return new Order(id, table, time, new List<OrderItems>(), comment, status);
        }

        private OrderItems ReadOrderItem(SqlDataReader reader)
        {
            return new OrderItems
            {
                Id = (int)reader["OrderItemID"],
                Name = (string)reader["MenuItemName"],
                Note = reader["Note"] == DBNull.Value ? null : (string)reader["Note"],
                courseType = Enum.TryParse((string)reader["Category"], out CourseType course) ? course : CourseType.Main,
                itemStatus = Enum.TryParse((string)reader["Status"], out ItemStatus status) ? status : ItemStatus.Pending,
                target = Enum.TryParse((string)reader["RoutingTarget"], out Order.RoutingTarget target) ? target : Order.RoutingTarget.Kitchen,
                menuType = Enum.TryParse((string)reader["MenuType"], out MenuType menuType) ? menuType : MenuType.All
            };
        }




        public List<Order> GetAllOrders(bool isKitchen, bool isReady)
        {
            var orders = new List<Order>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            string query = @"
        SELECT o.OrderId, o.TableID, o.Comment, o.OrderTime, o.OrderStatus,
               oi.OrderItemID, oi.Note, mi.Name AS MenuItemName, mi.Category,
               oi.Status, mi.RoutingTarget, mi.MenuType
        FROM Orders o
        JOIN OrderItems oi ON o.OrderId = oi.OrderId
        JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
        WHERE mi.RoutingTarget = @RoutingTarget AND o.OrderStatus = @OrderStatus
        ORDER BY o.OrderTime";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@RoutingTarget", isKitchen ? "Kitchen" : "Bar");
            cmd.Parameters.AddWithValue("@OrderStatus", isReady ? "Running" : "Finished");

            using var reader = cmd.ExecuteReader();

            Order? currentOrder = null;
            int? lastOrderId = null;

            while (reader.Read())
            {
                int orderId = (int)reader["OrderId"];

                if (lastOrderId == null || orderId != lastOrderId)
                {
                    currentOrder = ReadOrder(reader);
                    orders.Add(currentOrder);
                    lastOrderId = orderId;
                }

                var item = ReadOrderItem(reader);
                currentOrder?.Items.Add(item);
            }

            return orders;
        }




        public void UpdateOrderItemStatus(int orderId,int orderItemId, ItemStatus newStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE OrderItems SET Status = @Status WHERE OrderId = @OrderId AND OrderItemID = @OrderItemID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus.ToString());
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.Parameters.AddWithValue("@OrderItemID", orderItemId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateOrderStatus(int orderItemId, OrderStatus newStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Orders SET OrderStatus = @Status WHERE OrderID = @OrderItemID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus.ToString());
                    cmd.Parameters.AddWithValue("@OrderItemID", orderItemId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public Order? GetOrderById(int id)
        {
            Order? order = null;

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            string query = @"
        SELECT o.OrderId, o.TableID, o.Comment, o.OrderTime, o.OrderStatus,
               oi.OrderItemID, oi.Note, mi.Name AS MenuItemName, mi.Category,
               oi.Status, mi.RoutingTarget, mi.MenuType
        FROM Orders o
        JOIN OrderItems oi ON o.OrderId = oi.OrderId
        JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
        WHERE o.OrderId = @OrderId";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderId", id);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (order == null)
                {
                    order = ReadOrder(reader);
                }

                var item = ReadOrderItem(reader);
                order.Items.Add(item);
            }

            return order;
        }

    }
}

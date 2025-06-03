using System.Data.SqlClient;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using static restaurant_Chapeau.Models.Orders;

namespace restaurant_Chapeau.Repositaries
{
    public class OrderManagementRepository : IOrderManagement
    {
        private readonly string _connectionString;

        public OrderManagementRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<Orders> GetAllOrders()
        {
            var orders = new List<Orders>();
            var orderLookup = new Dictionary<int, Orders>();

            // Step 1: Get all invoiced order IDs from Invoice table
            var invoicedOrderIds = new HashSet<int>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT OrderId FROM Invoices", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    invoicedOrderIds.Add((int)reader["OrderId"]);
                }
            }

            // Step 2: Fetch all orders and their items
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                SELECT o.OrderId, o.TableID, o.Comment, o.OrderTime, mi.Name AS MenuItemName, mi.Category, oi.Status, oi.OrderItemID

                FROM Orders o
                JOIN OrderItems oi ON o.OrderId = oi.OrderId
                JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
                ORDER BY o.OrderTime";

                using var cmd = new SqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int orderId = (int)reader["OrderId"];

                    if (!orderLookup.TryGetValue(orderId, out Orders order))
                    {
                        order = new Orders
                        {
                            Id = orderId,
                            TableNumber = (int)reader["TableID"],
                            comment = reader["Comment"]?.ToString(),
                            OrderTime = (DateTime)reader["OrderTime"],
                            Items = new List<OrderItems>(),
                            Status = invoicedOrderIds.Contains(orderId)
                                        ? OrderStatus.Finished
                                        : OrderStatus.Running
                        };
                        orderLookup[orderId] = order;
                        orders.Add(order);
                    }

                    order.Items.Add(new OrderItems
                    {
                        Name = reader["MenuItemName"].ToString(),
                        Id = (int)reader["OrderItemID"],
                        courseType = Enum.TryParse<CourseType>(reader["Category"].ToString(), out var ct) ? ct : default,
                        itemStatus = Enum.TryParse<ItemStatus>(reader["Status"].ToString(), out var status) ? status : ItemStatus.Pending // or your default
                    });
                }

            }
                return orders;
        }

        // Now you don't need to filter by status later, but you can still use these if you want
        public List<Orders> GetRunningOrders(List<Orders> orders)
        {
            return orders.Where(order => order.Status == OrderStatus.Running).ToList();
        }

        public List<Orders> GetFinishedOrders(List<Orders> orders)
        {
            return orders.Where(order => order.Status == OrderStatus.Finished).ToList();
        }


        public Orders GetOrderById(int orderId)

        {
            using SqlConnection conn = new(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM Orders WHERE OrderId = @id", conn);
            cmd.Parameters.AddWithValue("@id", orderId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Orders
                {
                   
                };
            }

            return null;
        }

        public void UpdateOrderItemStatus(int orderItemId, OrderStatus newStatus)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Save status as a string: "Pending", "Preparing", "Ready", "Served"
            var cmd = new SqlCommand("UPDATE OrderItems SET Status = @status WHERE OrderItemId = @itemId", conn);
            cmd.Parameters.AddWithValue("@status", newStatus.ToString());
            cmd.Parameters.AddWithValue("@itemId", orderItemId);

            cmd.ExecuteNonQuery();
        }

        
    }
}

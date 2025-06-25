using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using restaurant_Chapeau.Enums;
using restaurant_Chapeau.Models;

using restaurant_Chapeau.Services.Interfaces;



namespace restaurant_Chapeau.Repositaries
{
    public class KitchenBarRepository : IKitchenBarRepository
    {
        private readonly string _connectionString;

        public KitchenBarRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<Order> GetRunningOrders()
        {
            var runningOrders = new List<Order>();
            var orderLookup = new Dictionary<int, Order>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                //
                string query = @"
                SELECT o.OrderId, o.TableID, o.Comment, o.OrderTime, oi.Note,
                       mi.Name AS MenuItemName, mi.Category, oi.Status, oi.OrderItemID, mi.RoutingTarget
                FROM Orders o
                JOIN OrderItems oi ON o.OrderId = oi.OrderId
                JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
                ORDER BY o.OrderTime";

                using var cmd = new SqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int orderId = (int)reader["OrderId"];

                    if (!orderLookup.TryGetValue(orderId, out Order order))
                    {
                        order = new Order
                        {
                            Id = orderId,
                            TableNumber = (int)reader["TableID"],
                            comment = reader["Comment"]?.ToString(),
                            OrderTime = (DateTime)reader["OrderTime"],
                            Items = new List<OrderItem>()
                        };
                        orderLookup[orderId] = order;
                    }

                    var itemStatus = Enum.TryParse<ItemStatus>(reader["Status"].ToString(), out var status)
                        ? status : ItemStatus.Pending;

                    order.Items.Add(new OrderItem
                    {
                        Id = (int)reader["OrderItemID"],
                        Note = reader["Note"] == DBNull.Value ? null : reader["Note"].ToString(),
                        Name = reader["MenuItemName"].ToString(),
                        courseType = Enum.TryParse<CourseType>(reader["Category"].ToString(), out var ct) ? ct : default,
                        itemStatus = itemStatus,
                        target = Enum.TryParse<RoutingTarget>(reader["RoutingTarget"].ToString(), out var target) ? target : default
                    });
                }
            }

            foreach (var order in orderLookup.Values)
            {
                if (order.Items.Any(item => item.itemStatus != ItemStatus.Ready))
                {
                    order.Status = OrderStatus.Running;
                    runningOrders.Add(order);
                }
            }

            return runningOrders;
        }

        public List<Order> GetFinishedOrders()
        {
            var finishedOrders = new List<Order>();
            var orderLookup = new Dictionary<int, Order>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                SELECT o.OrderId, o.TableID, o.Comment, o.OrderTime, oi.Note,
                       mi.Name AS MenuItemName, mi.Category, oi.Status, oi.OrderItemID
                FROM Orders o
                JOIN OrderItems oi ON o.OrderId = oi.OrderId
                JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
                ORDER BY o.OrderTime";

                using var cmd = new SqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int orderId = (int)reader["OrderId"];

                    if (!orderLookup.TryGetValue(orderId, out Order order))
                    {
                        order = new Order
                        {
                            Id = orderId,
                            TableNumber = (int)reader["TableID"],
                            comment = reader["Comment"]?.ToString(),
                            OrderTime = (DateTime)reader["OrderTime"],
                            Items = new List<OrderItem>()
                        };
                        orderLookup[orderId] = order;
                    }

                    var itemStatus = Enum.TryParse<ItemStatus>(reader["Status"].ToString(), out var status)
                        ? status : ItemStatus.Pending;

                    order.Items.Add(new OrderItem
                    {
                        Id = (int)reader["OrderItemID"],
                        Note = reader["Note"] == DBNull.Value ? null : reader["Note"].ToString(),
                        Name = reader["MenuItemName"].ToString(),
                        courseType = Enum.TryParse<CourseType>(reader["Category"].ToString(), out var ct) ? ct : default,
                        itemStatus = itemStatus
                    });
                }
            }

            foreach (var order in orderLookup.Values)
            {
                if (order.Items.All(item => item.itemStatus == ItemStatus.Ready))
                {
                    order.Status = OrderStatus.Finished;
                    finishedOrders.Add(order);
                }
            }

            return finishedOrders;
        }

        public Order GetOrderById(int orderId)
        {
            var orderLookup = new Dictionary<int, Order>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                SELECT o.OrderId, o.TableID, o.Comment, o.OrderTime, oi.Note,
                       mi.Name AS MenuItemName, mi.Category, oi.Status, oi.OrderItemID, mi.RoutingTarget
                FROM Orders o
                JOIN OrderItems oi ON o.OrderId = oi.OrderId
                JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
                WHERE o.OrderId = @orderId
                ORDER BY o.OrderTime";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@orderId", orderId);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int currentOrderId = (int)reader["OrderId"];

                    if (!orderLookup.TryGetValue(currentOrderId, out Order order))
                    {
                        order = new Order
                        {
                            Id = currentOrderId,
                            TableNumber = (int)reader["TableID"],
                            comment = reader["Comment"]?.ToString(),
                            OrderTime = (DateTime)reader["OrderTime"],
                            Items = new List<OrderItem>()
                        };
                        orderLookup[currentOrderId] = order;
                    }

                    var itemStatus = Enum.TryParse<ItemStatus>(reader["Status"].ToString(), out var status)
                        ? status : ItemStatus.Pending;

                    order.Items.Add(new OrderItem
                    {
                        Id = (int)reader["OrderItemID"],
                        Note = reader["Note"] == DBNull.Value ? null : reader["Note"].ToString(),
                        Name = reader["MenuItemName"].ToString(),
                        courseType = Enum.TryParse<CourseType>(reader["Category"].ToString(), out var ct) ? ct : default,
                        itemStatus = itemStatus,
                        target = Enum.TryParse<RoutingTarget>(reader["RoutingTarget"].ToString(), out var target) ? target : default
                    });
                }
            }

            return orderLookup.Values.FirstOrDefault();
        }

        public void UpdateOrderItemStatus(int orderItemId, ItemStatus newStatus)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("UPDATE OrderItems SET Status = @status WHERE OrderItemId = @itemId", conn);
            cmd.Parameters.AddWithValue("@status", newStatus.ToString());
            cmd.Parameters.AddWithValue("@itemId", orderItemId);

            cmd.ExecuteNonQuery();
        }
    }
}

using restaurant_Chapeau.Models;
using restaurant_Chapeau.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using static restaurant_Chapeau.Models.Order;

namespace restaurant_Chapeau.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;
        private const int CommandTimeout = 60; // 60 seconds timeout

        public PaymentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public TableOrderView GetCompleteOrderByTableId(int tableId)
        {
            var view = new TableOrderView
            {
                TableID = tableId,
                TableNumber = tableId,
                Items = new List<OrderItemView>(),
                TotalLowVAT = 0,
                TotalHighVAT = 0,
                TotalAmount = 0,
                OrderID = 0
            };

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Simple, fast query to get the most recent unpaid order
                    var orderCmd = new SqlCommand(@"
                        SELECT TOP 1 OrderID
                        FROM Orders 
                        WHERE TableID = @tableId 
                          AND (IsPaid IS NULL OR IsPaid = 0)
                        ORDER BY OrderTime DESC", conn);

                    orderCmd.Parameters.AddWithValue("@tableId", tableId);
                    orderCmd.CommandTimeout = CommandTimeout;

                    var orderIdResult = orderCmd.ExecuteScalar();
                    if (orderIdResult == null)
                    {
                        return view; // No unpaid orders found
                    }

                    view.OrderID = Convert.ToInt32(orderIdResult);

                    // Optimized query to get order items directly by OrderID
                    var itemsCmd = new SqlCommand(@"
                        SELECT 
                            mi.Name, 
                            SUM(oi.Quantity) as TotalQuantity, 
                            mi.Price, 
                            mi.VATRate
                        FROM OrderItems oi
                        INNER JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                        WHERE oi.OrderID = @orderId
                        GROUP BY mi.Name, mi.Price, mi.VATRate", conn);

                    itemsCmd.Parameters.AddWithValue("@orderId", view.OrderID);
                    itemsCmd.CommandTimeout = CommandTimeout;

                    using (var reader = itemsCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString("Name");
                            int qty = reader.GetInt32("TotalQuantity");
                            decimal price = reader.GetDecimal("Price");
                            decimal vatRate = reader.GetDecimal("VATRate");

                            decimal totalPrice = qty * price;
                            decimal vatAmount = totalPrice * (vatRate / 100);

                            var item = new OrderItemView
                            {
                                ItemName = name,
                                Quantity = qty,
                                TotalPrice = totalPrice,
                                VATRate = vatRate
                            };

                            if (vatRate == 21)
                                view.TotalHighVAT += vatAmount;
                            else if (vatRate == 9)
                                view.TotalLowVAT += vatAmount;

                            view.TotalAmount += totalPrice;
                            view.Items.Add(item);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log the error and return empty view
                Console.WriteLine($"SQL Error in GetCompleteOrderByTableId: {ex.Message}");
                return view;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCompleteOrderByTableId: {ex.Message}");
                return view;
            }

            return view;
        }

        public int GetOpenOrderIdByTable(int tableId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        SELECT TOP 1 OrderID
                        FROM Orders
                        WHERE TableID = @tableId 
                          AND (IsPaid IS NULL OR IsPaid = 0)
                        ORDER BY OrderTime DESC", conn);

                    cmd.Parameters.AddWithValue("@tableId", tableId);
                    cmd.CommandTimeout = CommandTimeout;

                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOpenOrderIdByTable: {ex.Message}");
                return -1;
            }
        }

        public decimal CalculateVATForOrder(int orderId)
        {
            decimal totalVAT = 0;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        SELECT ISNULL(SUM(oi.Quantity * mi.Price * (mi.VATRate / 100)), 0) as TotalVAT
                        FROM OrderItems oi
                        INNER JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                        WHERE oi.OrderID = @orderId", conn);

                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.CommandTimeout = CommandTimeout;

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                        totalVAT = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateVATForOrder: {ex.Message}");
            }

            return totalVAT;
        }

        public void CreateInvoice(Invoice invoice)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        INSERT INTO Invoices (InvoiceNumber, OrderID, UserID, TotalAmount, TipAmount, VATAmount, CreatedAt)
                        VALUES (@InvoiceNumber, @OrderID, @UserID, @TotalAmount, @TipAmount, @VATAmount, GETDATE())", conn);

                    cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                    cmd.Parameters.AddWithValue("@OrderID", invoice.OrderID);
                    cmd.Parameters.AddWithValue("@UserID", invoice.UserID);
                    cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);
                    cmd.Parameters.AddWithValue("@TipAmount", invoice.TipAmount);
                    cmd.Parameters.AddWithValue("@VATAmount", invoice.VATAmount);
                    cmd.CommandTimeout = CommandTimeout;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create invoice: {ex.Message}", ex);
            }
        }

        public void SaveSplitPayment(int orderId, decimal amount, string method, string feedback)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Create invoice for this split payment
                    var cmd = new SqlCommand(@"
                        INSERT INTO Invoices (InvoiceNumber, OrderID, UserID, TotalAmount, TipAmount, VATAmount, CreatedAt)
                        VALUES (@InvoiceNumber, @OrderID, @UserID, @TotalAmount, @TipAmount, @VATAmount, GETDATE())", conn);

                    cmd.Parameters.AddWithValue("@InvoiceNumber", $"SPLIT-{Guid.NewGuid().ToString().Substring(0, 8)}");
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@UserID", 1);
                    cmd.Parameters.AddWithValue("@TotalAmount", amount);
                    cmd.Parameters.AddWithValue("@TipAmount", 0);
                    cmd.Parameters.AddWithValue("@VATAmount", 0); // Calculate proportional VAT if needed
                    cmd.CommandTimeout = CommandTimeout;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save split payment: {ex.Message}", ex);
            }
        }

        public decimal GetTotalAmountForOrder(int orderId)
        {
            decimal total = 0;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new SqlCommand(@"
                        SELECT ISNULL(SUM(oi.Quantity * mi.Price), 0)
                        FROM OrderItems oi
                        INNER JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                        WHERE oi.OrderID = @orderId", conn);

                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.CommandTimeout = CommandTimeout;

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        total = Convert.ToDecimal(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTotalAmountForOrder: {ex.Message}");
            }

            return total;
        }

        public void FreeTable(int tableId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        UPDATE RestaurantTables 
                        SET IsReserved = 0, ReservationStart = NULL, ReservationEnd = NULL 
                        WHERE TableID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", tableId);
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to free table: {ex.Message}", ex);
            }
        }

        public void MarkOrderAsPaid(int orderId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("UPDATE Orders SET IsPaid = 1 WHERE OrderID = @orderId", conn);
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to mark order as paid: {ex.Message}", ex);
            }
        }

        public List<RestaurantTable> GetTablesWithUnpaidOrders()
        {
            var tables = new List<RestaurantTable>();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        SELECT DISTINCT rt.TableID, rt.TableNumber, rt.IsReserved, rt.ReservationStart, rt.ReservationEnd
                        FROM RestaurantTables rt
                        INNER JOIN Orders o ON rt.TableID = o.TableID
                        WHERE (o.IsPaid IS NULL OR o.IsPaid = 0)", conn);

                    cmd.CommandTimeout = CommandTimeout;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(new RestaurantTable
                            {
                                TableID = reader.GetInt32("TableID"),
                                TableNumber = reader.GetInt32("TableNumber"),
                                IsReserved = reader.IsDBNull("IsReserved") ? false : reader.GetBoolean("IsReserved"),
                                ReservationStart = reader["ReservationStart"] != DBNull.Value ? (DateTime?)reader["ReservationStart"] : null,
                                ReservationEnd = reader["ReservationEnd"] != DBNull.Value ? (DateTime?)reader["ReservationEnd"] : null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTablesWithUnpaidOrders: {ex.Message}");
            }

            return tables;
        }
    }
}
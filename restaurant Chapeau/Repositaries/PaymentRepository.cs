using restaurant_Chapeau.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using static restaurant_Chapeau.Models.Order;

namespace restaurant_Chapeau.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }


        public TableOrderView GetCompleteOrderByTableId(int tableId)
        {
            var view = new TableOrderView
            {
                TableNumber = tableId,
                Items = new List<OrderItemView>(),
                TotalLowVAT = 0,
                TotalHighVAT = 0,
                TotalAmount = 0
            };

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT mi.Name, oi.Quantity, mi.Price, mi.VATRate
                    FROM Orders o
                    JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    WHERE o.TableID = @tableId 
                      AND o.OrderID NOT IN (SELECT OrderID FROM Invoices)", conn);

                cmd.Parameters.AddWithValue("@tableId", tableId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        int qty = reader.GetInt32(1);
                        decimal price = reader.GetDecimal(2);
                        decimal vatRate = reader.GetDecimal(3);

                        decimal totalPrice = qty * price;

                        var item = new OrderItemView
                        {
                            ItemName = name,
                            Quantity = qty,
                            TotalPrice = totalPrice,
                            VATRate = vatRate
                        };

                        if (vatRate == 21)
                            view.TotalHighVAT += totalPrice * 0.21m;
                        else
                            view.TotalLowVAT += totalPrice * 0.09m;

                        view.TotalAmount += totalPrice;
                        view.Items.Add(item);
                    }
                }
            }

            return view;
        }

        public int GetOpenOrderIdByTable(int tableId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
            SELECT TOP 1 o.OrderID
            FROM Orders o
            WHERE o.TableID = @tableId
              AND (
                  SELECT ISNULL(SUM(i.TotalAmount), 0)
                  FROM Invoices i
                  WHERE i.OrderID = o.OrderID
              ) < (
                  SELECT SUM(oi.Quantity * mi.Price)
                  FROM OrderItems oi
                  JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                  WHERE oi.OrderID = o.OrderID
              )", conn); 
        
        cmd.Parameters.AddWithValue("@tableId", tableId);

                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }



        public decimal CalculateVATForOrder(int orderId)
        {
            // Dummy VAT value; adjust logic per item VAT in production
            return 0.21m;
        }

        public void CreateInvoice(Invoice invoice)
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

                cmd.ExecuteNonQuery();
            }
        }

        public void SaveSplitPayment(int orderId, decimal amount, string method, string feedback)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Orders WHERE OrderID = @orderId", conn);
                checkCmd.Parameters.AddWithValue("@orderId", orderId);
                int count = (int)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    throw new Exception($"❌ OrderID {orderId} does not exist in the Orders table. Cannot save invoice.");
                }

                var cmd = new SqlCommand(@"
                    INSERT INTO Invoices (InvoiceNumber, OrderID, UserID, TotalAmount, TipAmount, VATAmount, CreatedAt)
                    VALUES (@InvoiceNumber, @OrderID, @UserID, @TotalAmount, @TipAmount, @VATAmount, GETDATE())", conn);

                cmd.Parameters.AddWithValue("@InvoiceNumber", Guid.NewGuid().ToString());
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@UserID", 1); // Replace with actual logged-in user ID if needed
                cmd.Parameters.AddWithValue("@TotalAmount", amount);
                cmd.Parameters.AddWithValue("@TipAmount", 0);
                cmd.Parameters.AddWithValue("@VATAmount", 0);

                cmd.ExecuteNonQuery();
            }
        }

        public decimal GetTotalAmountForOrder(int orderId)
        {
            decimal total = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
            SELECT SUM(oi.Quantity * mi.Price)
            FROM OrderItems oi
            INNER JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
            WHERE oi.OrderID = @orderId", conn);

                cmd.Parameters.AddWithValue("@orderId", orderId);

                var result = cmd.ExecuteScalar();

                if (result != DBNull.Value && result != null)
                {
                    total = Convert.ToDecimal(result);
                }

                Console.WriteLine($"🧪 Order {orderId} has total: {total}");
            }

            return total;
        }



        public void FreeTable(int tableId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE RestaurantTables SET IsReserved = 0, ReservationStart = NULL, ReservationEnd = NULL WHERE TableID = @id", conn);
                cmd.Parameters.AddWithValue("@id", tableId);
                cmd.ExecuteNonQuery();
            }
        }

        public void MarkOrderAsPaid(int orderId)
        {
            // Optional: Add flag in Orders to mark as paid if needed
        }

        public List<RestaurantTable> GetTablesWithUnpaidOrders()
        {
            var tables = new List<RestaurantTable>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT DISTINCT t.TableID, t.TableNumber
                    FROM RestaurantTables t
                    JOIN Orders o ON t.TableID = o.TableID
                    WHERE o.OrderID NOT IN (SELECT OrderID FROM Invoices)", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(new RestaurantTable
                        {
                            TableID = reader.GetInt32(0),
                            TableNumber = reader.GetInt32(1),
                        });
                    }
                }
            }

            return tables;
        }
    }
}

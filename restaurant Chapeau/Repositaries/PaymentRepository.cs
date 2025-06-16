using System.Data.SqlClient;
using System.Numerics;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositories;

namespace restaurant_Chapeau.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<TableOrderView> GetCompleteOrderByTableIdAsync(int tableId)
        {
            var model = new TableOrderView
            {
                TableID = tableId,
                Items = new List<OrderItemView>()
            };

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Get table number
            var getTableCmd = new SqlCommand("SELECT TableNumber FROM RestaurantTables WHERE TableID = @TableID", connection);
            getTableCmd.Parameters.AddWithValue("@TableID", tableId);
            var tableNumberObj = await getTableCmd.ExecuteScalarAsync();
            if (tableNumberObj == null) return model;

            model.TableNumber = (int)tableNumberObj;

            // Get latest unpaid order
            var getOrderCmd = new SqlCommand(@"
                SELECT TOP 1 OrderID 
                FROM Orders 
                WHERE TableID = @TableID AND IsPaid = 0 
                ORDER BY OrderTime DESC", connection);
            getOrderCmd.Parameters.AddWithValue("@TableID", tableId);
            var orderIdObj = await getOrderCmd.ExecuteScalarAsync();
            if (orderIdObj == null) return model;

            int orderId = (int)orderIdObj;

            // Get order items
            var cmd = new SqlCommand(@"
                SELECT mi.Name, SUM(oi.Quantity) AS TotalQuantity, mi.Price, mi.VATRate
                FROM OrderItems oi
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                WHERE oi.OrderID = @OrderID
                GROUP BY mi.Name, mi.Price, mi.VATRate", connection);
            cmd.Parameters.AddWithValue("@OrderID", orderId);

            decimal total = 0, totalLowVAT = 0, totalHighVAT = 0;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string name = reader["Name"].ToString();
                int qty = (int)reader["TotalQuantity"];
                decimal price = (decimal)reader["Price"];
                decimal vat = (decimal)reader["VATRate"];
                decimal subtotal = qty * price;

                model.Items.Add(new OrderItemView
                {
                    ItemName = name,
                    Quantity = qty,
                    TotalPrice = subtotal,
                    VATRate = vat
                });

                total += subtotal;

                if (vat == 9.00M)
                    totalLowVAT += subtotal * vat / 100;
                else if (vat == 21.00M)
                    totalHighVAT += subtotal * vat / 100;
            }

            model.TotalAmount = total;
            model.TotalLowVAT = totalLowVAT;
            model.TotalHighVAT = totalHighVAT;

            return model;
        }

        public async Task<int> GetOpenOrderIdByTableAsync(int tableId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
                SELECT TOP 1 OrderID 
                FROM Orders 
                WHERE TableID = @TableID AND IsPaid = 0 
                ORDER BY OrderTime DESC", conn);
            cmd.Parameters.AddWithValue("@TableID", tableId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        public async Task<decimal> CalculateVATForOrderAsync(int orderId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
                SELECT SUM(oi.Quantity * mi.Price * mi.VATRate / 100.0)
                FROM OrderItems oi
                JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                WHERE oi.OrderID = @OrderID", conn);
            cmd.Parameters.AddWithValue("@OrderID", orderId);
            return (decimal)(await cmd.ExecuteScalarAsync() ?? 0);
        }

        public async Task CreateInvoiceAsync(Invoice invoice)
        {
            decimal CostAmount = invoice.TotalAmount * 0.6m; 
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"INSERT INTO Invoices (InvoiceNumber, OrderID, UserID, TotalAmount, TipAmount, VATAmount, CostAmount, CreatedAt)
                                     VALUES (@InvoiceNumber, @OrderID, @UserID, @TotalAmount, @TipAmount, @VATAmount, @CostAmount, GETDATE())", conn);

            cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
            cmd.Parameters.AddWithValue("@OrderID", invoice.OrderID);
            cmd.Parameters.AddWithValue("@UserID", invoice.UserID);
            cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);
            cmd.Parameters.AddWithValue("@TipAmount", invoice.TipAmount);
            cmd.Parameters.AddWithValue("@VATAmount", invoice.VATAmount);
            cmd.Parameters.AddWithValue("@CostAmount", CostAmount);

            await cmd.ExecuteNonQueryAsync();


            // Mark the order as paid
            var updateCmd = new SqlCommand("UPDATE Orders SET IsPaid = 1 WHERE OrderID = @OrderID", conn);
            updateCmd.Parameters.AddWithValue("@OrderID", invoice.OrderID);
            await updateCmd.ExecuteNonQueryAsync();
        }

        public async Task SaveSplitPaymentAsync(int orderId, decimal amount, string method, string feedback)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
                INSERT INTO Payments (OrderID, Amount, PaymentMethod, Feedback)
                VALUES (@OrderID, @Amount, @Method, @Feedback)", conn);
            cmd.Parameters.AddWithValue("@OrderID", orderId);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Method", method);
            cmd.Parameters.AddWithValue("@Feedback", feedback ?? "");
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task FreeTableAsync(int tableId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"
                UPDATE RestaurantTables
                SET IsReserved = 0, ReservationStart = NULL, ReservationEnd = NULL
                WHERE TableID = @TableID", conn);
            cmd.Parameters.AddWithValue("@TableID", tableId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkOrderAsPaidAsync(int orderId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE Orders SET IsPaid = 1 WHERE OrderID = @OrderID", conn);
            cmd.Parameters.AddWithValue("@OrderID", orderId);
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<List<RestaurantTable>> GetTablesWithUnpaidOrdersAsync()
        {
            var tables = new List<RestaurantTable>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT DISTINCT rt.TableID, rt.TableNumber
                FROM RestaurantTables rt
                JOIN Orders o ON rt.TableID = o.TableID
                WHERE o.IsPaid = 0", conn);

            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(new RestaurantTable
                {
                    TableID = reader.GetInt32(0),
                    TableNumber = reader.GetInt32(1)
                });
            }

            return tables;
        }
    }
}

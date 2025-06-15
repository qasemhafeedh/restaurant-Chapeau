using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace restaurant_Chapeau.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<Invoice> GetInvoicesByDateRange(DateTime startDate, DateTime endDate)
        {
            List<Invoice> invoices = new List<Invoice>();

            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Invoices WHERE CreatedAt BETWEEN @start AND @end", conn);
            cmd.Parameters.AddWithValue("@start", startDate);
            cmd.Parameters.AddWithValue("@end", endDate);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                invoices.Add(new Invoice
                {
                    InvoiceID = (int)reader["InvoiceID"],
                    InvoiceNumber = reader["InvoiceNumber"].ToString(),
                    OrderID = (int)reader["OrderID"],
                    UserID = (int)reader["UserID"],
                    TotalAmount = (decimal)reader["TotalAmount"],
                    TipAmount = (decimal)reader["TipAmount"],
                    VATAmount = (decimal)reader["VATAmount"],
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    CostAmount = (decimal)reader["CostAmount"]
                });
            }

            return invoices;
        }
    }
}

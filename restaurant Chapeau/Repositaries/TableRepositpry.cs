using System.Data.SqlClient;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;

namespace restaurant_Chapeau.Repositories
{
    public class TableRepository : ITableRepository
    {
        private readonly string _connectionString;

        public TableRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<RestaurantTable>> GetAllTablesAsync()
        {
            var tables = new List<RestaurantTable>();

            using SqlConnection conn = new(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT TableID, TableNumber,IsReserved, ReservationStart, ReservationEnd FROM RestaurantTables", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(MapToTable(reader));
            }

            return tables;
        }

        public async Task<bool> IsReservedAsync(int tableId)
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

       

        private RestaurantTable MapToTable(SqlDataReader reader)
        {
            return new RestaurantTable
            {
                TableID = (int)reader["TableID"],
                TableNumber = (int)reader["TableNumber"],
                IsReserved = (bool)reader["IsReserved"],
                ReservationStart = reader["ReservationStart"] != DBNull.Value ? (DateTime?)reader["ReservationStart"] : null,
                ReservationEnd = reader["ReservationEnd"] != DBNull.Value ? (DateTime?)reader["ReservationEnd"] : null
            };
        }
    }
}

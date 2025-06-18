using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    /// <summary>
    /// Provides business logic for handling restaurant table operations.
    /// </summary>
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableService"/> class.
        /// </summary>
        /// <param name="tableRepository">The repository used for accessing table data.</param>
        public TableService(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// Retrieves all restaurant tables.
        /// </summary>
        /// <returns>A list of all <see cref="RestaurantTable"/> objects.</returns>
        public Task<List<RestaurantTable>> GetAllTablesAsync()
        {
            return _tableRepository.GetAllTablesAsync();
        }

        /// <summary>
        /// Determines whether the specified table is currently reserved.
        /// </summary>
        /// <param name="tableId">The unique identifier of the table.</param>
        /// <returns><c>true</c> if the table is reserved; otherwise, <c>false</c>.</returns>
        public Task<bool> IsReservedAsync(int tableId)
        {
            return _tableRepository.IsReservedAsync(tableId);
        }

        /// <summary>
        /// Reserves the table for the current time.
        /// </summary>
        /// <param name="tableId">The unique identifier of the table to reserve.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task ReserveAsync(int tableId)
        {
            return _tableRepository.ReserveAsync(tableId);
        }
        public async Task CleanupExpiredReservationsAsync()
        {
            await _tableRepository.CleanupExpiredReservationsAsync();
        }



    }
}

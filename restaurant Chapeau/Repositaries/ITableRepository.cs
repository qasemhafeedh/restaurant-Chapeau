namespace restaurant_Chapeau.Repositaries
{
    // Interfaces/ITableRepository.cs
    using restaurant_Chapeau.Models;

    public interface ITableRepository
    {
        Task<List<RestaurantTable>> GetAllTablesAsync();
        Task<bool> IsReservedAsync(int tableId);
     
        Task ReserveAsync(int tableId);
        Task CleanupExpiredReservationsAsync();

    }

}

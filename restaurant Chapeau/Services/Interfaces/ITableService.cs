namespace restaurant_Chapeau.Services.Interfaces
{
    // Services/Interfaces/ITableService.cs
    using restaurant_Chapeau.Models;

    public interface ITableService
    {
        Task<List<RestaurantTable>> GetAllTablesAsync();
        Task<bool> IsReservedAsync(int tableId);
        
       

    }

}

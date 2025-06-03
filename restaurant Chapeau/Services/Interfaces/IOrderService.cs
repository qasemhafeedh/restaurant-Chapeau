using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<MenuItem>> GetMenuItemsAsync();
        Task<List<RestaurantTable>> GetTablesAsync();
        Task<bool> SubmitOrderAsync(OrderSubmission model, int userId);

    }
}

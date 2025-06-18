using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IMenuItemService
    {
        Task<List<MenuItem>> GetAllMenuItemsAsync();
        Task<bool> IsStockAvailableAsync(int menuItemId, int quantity);
        Task DecreaseStockAsync(int menuItemId, int quantity);
        Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId);

    }

}


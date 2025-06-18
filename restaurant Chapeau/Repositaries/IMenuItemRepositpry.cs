using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositaries
{
    public interface IMenuItemRepository
    {
        Task<List<MenuItem>> GetAllAsync();
        Task<bool> IsStockAvailableAsync(int menuItemId, int quantity);
        Task DecreaseStockAsync(int menuItemId, int quantity);
        Task<MenuItem?> GetByIdAsync(int menuItemId);

    }
}


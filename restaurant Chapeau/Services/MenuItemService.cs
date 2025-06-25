using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository;

        public MenuItemService(IMenuItemRepository menuItemRepository)
        {
            _menuItemRepository = menuItemRepository;
        }

       
        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            try
            {
                return await _menuItemRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                // Log the error before rethrowing if logging is configured.
                throw new ApplicationException("An error occurred while loading menu items.", ex);
            }
        }

        
        public async Task<bool> IsStockAvailableAsync(int menuItemId, int quantity)
        {
            return await _menuItemRepository.IsStockAvailableAsync(menuItemId, quantity);
        }

      
        public async Task DecreaseStockAsync(int menuItemId, int quantity)
        {
            await _menuItemRepository.DecreaseStockAsync(menuItemId, quantity);
        }
        public async Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId)
        {
            List<MenuItem> allItems = await _menuItemRepository.GetAllAsync();
            return allItems.FirstOrDefault(m => m.MenuItemID == menuItemId);
        }

    }
}

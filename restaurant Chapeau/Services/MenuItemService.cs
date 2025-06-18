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

        /// <summary>
        /// Fetches all menu items from the repository.
        /// </summary>
        /// <returns>List of menu items</returns>
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

        /// <summary>
        /// Checks if sufficient stock exists for a specific menu item.
        /// </summary>
        /// <param name="menuItemId">Menu item ID</param>
        /// <param name="quantity">Requested quantity</param>
        /// <returns>True if stock is sufficient, otherwise false</returns>
        public async Task<bool> IsStockAvailableAsync(int menuItemId, int quantity)
        {
            return await _menuItemRepository.IsStockAvailableAsync(menuItemId, quantity);
        }

        /// <summary>
        /// Decreases the stock of a menu item by the specified quantity.
        /// </summary>
        /// <param name="menuItemId">Menu item ID</param>
        /// <param name="quantity">Quantity to decrease</param>
        public async Task DecreaseStockAsync(int menuItemId, int quantity)
        {
            await _menuItemRepository.DecreaseStockAsync(menuItemId, quantity);
        }
        public async Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId)
        {
            var allItems = await _menuItemRepository.GetAllAsync();
            return allItems.FirstOrDefault(m => m.MenuItemID == menuItemId);
        }

    }
}

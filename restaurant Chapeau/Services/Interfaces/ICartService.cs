using restaurant_Chapeau.Models;
using restaurant_Chapeau.ViewModels;

public interface ICartService
{
    void AddOrUpdateItem(List<CartItem> cart, MenuItem menuItem, AddToCartViewModel model);
    Task<AddToCartResult> AddToCartAsync(AddToCartViewModel model);
    List<CartItem> GetCartItems();
    void SaveCartItems(List<CartItem> items);
    CartViewModel BuildCartViewModel(List<RestaurantTable> availableTables);
    void ClearCart();
    void RemoveItem(int id);
    
}


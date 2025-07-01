using restaurant_Chapeau.Models;
using restaurant_Chapeau.ViewModels;
namespace restaurant_Chapeau.Services.Interfaces
{
   

    public interface ICartService
    {
        void AddOrUpdateItem(List<CartItem> cart, MenuItem menuItem, AddToCartViewModel model);
        Task<AddToCartResult> AddToCartAsync(AddToCartViewModel model);
        List<CartItem> GetCartItems();
        void SaveCartItems(List<CartItem> items);
        List<CartItemViewModel> BuildCartItemViewModels();
        void ClearCart();
        void RemoveItem(int id);

    }
}

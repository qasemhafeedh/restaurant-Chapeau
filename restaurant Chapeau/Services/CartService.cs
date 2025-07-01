using Microsoft.AspNetCore.Http;
using restaurant_Chapeau.Helpers;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.ViewModels;
using restaurant_Chapeau.Enums;
using restaurant_Chapeau.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CartService : ICartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMenuItemService _menuItemService;

    public CartService(IHttpContextAccessor httpContextAccessor, IMenuItemService menuItemService)
    {
        _httpContextAccessor = httpContextAccessor;
        _menuItemService = menuItemService;
    }

    public async Task<AddToCartResult> AddToCartAsync(AddToCartViewModel model)
    {
        MenuItem menuItem = await _menuItemService.GetMenuItemByIdAsync(model.MenuItemID);

        if (!IsValidCartRequest(menuItem, model))
        {
            // return new AddToCartResult { StatusMessage = " Invalid item or quantity." }; /// use throw exception
            throw new InvalidOperationException("Invalid item or quantity.");
        }

        // accessing the session
        ISession session = _httpContextAccessor.HttpContext.Session;
        List<CartItem> cart = session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        AddOrUpdateItem(cart, menuItem, model);
        session.SetObjectAsJson("Cart", cart);

        return new AddToCartResult { StatusMessage = " Item added to cart!" };
    }

    public void AddOrUpdateItem(List<CartItem> cart, MenuItem menuItem, AddToCartViewModel model)
    {
        // check if the item is already in the cart or not 
        CartItem existingItem = cart.FirstOrDefault(i => i.MenuItemID == model.MenuItemID);

        if (existingItem != null)
        {
            existingItem.Quantity += model.Quantity; // modifies the quantity and reflects the changes to the cart

            if (!string.IsNullOrWhiteSpace(model.Note))
            {
                if (!string.IsNullOrWhiteSpace(existingItem.Note))
                    existingItem.Note += "\n" + model.Note;
                else
                    existingItem.Note = model.Note;
            }
        }
        else
        {
            cart.Add(MapToCartItem(menuItem, model));
        }
    }

    private bool IsValidCartRequest(MenuItem? menuItem, AddToCartViewModel model)
    {
        return menuItem != null &&
               model.Quantity > 0 &&
               model.Quantity <= menuItem.QuantityAvailable;
    }

    private CartItem MapToCartItem(MenuItem menuItem, AddToCartViewModel model)
    {
        return new CartItem
        {
            MenuItemID = menuItem.MenuItemID,
            Name = menuItem.Name,
            Price = menuItem.Price,
            Quantity = model.Quantity, // we need it  user input
            Note = model.Note,         // user input interface
            RoutingTarget = menuItem.RoutingTarget.ToString()
        };
    }

    public List<CartItem> GetCartItems()
    {
        return _httpContextAccessor.HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
    }

    public void SaveCartItems(List<CartItem> items)
    {
        _httpContextAccessor.HttpContext.Session.SetObjectAsJson("Cart", items);
    }

    // to remove single item from the cart
    public void RemoveItem(int id)
    {
        ISession session = _httpContextAccessor.HttpContext.Session;
        List<CartItem> cart = session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        cart.RemoveAll(i => i.MenuItemID == id);
        session.SetObjectAsJson("Cart", cart);
    }

    // to delete the whole cart 
    public void ClearCart()
    {
        _httpContextAccessor.HttpContext.Session.Remove("Cart");
    }

    public List<CartItemViewModel> BuildCartItemViewModels()
    {
        // Get the items currently saved in the cart from session
        List<CartItem> itemsInCart = GetCartItems();

        // Convert each CartItem into a simpler view model for the page
        List<CartItemViewModel> viewModelItems = new List<CartItemViewModel>();

        foreach (CartItem item in itemsInCart)
        {
            CartItemViewModel viewModel = new CartItemViewModel
            {
                MenuItemID = item.MenuItemID,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                Note = item.Note,
                RoutingTarget = Enum.Parse<RoutingTarget>(item.RoutingTarget)
            };

            viewModelItems.Add(viewModel);
        }

        return viewModelItems;
    }
}

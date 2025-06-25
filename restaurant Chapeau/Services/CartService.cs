
using Microsoft.AspNetCore.Http;
using restaurant_Chapeau.Helpers;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.ViewModels;
using restaurant_Chapeau.Enums;
using restaurant_Chapeau.Services.Interfaces;
using Microsoft.AspNetCore.Cors.Infrastructure;

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
        var menuItem = await _menuItemService.GetMenuItemByIdAsync(model.MenuItemID);

        if (!IsValidCartRequest(menuItem, model))
        {
            return new AddToCartResult { StatusMessage = "⚠️ Invalid item or quantity." };
        }
        // accessing the session
        var session = _httpContextAccessor.HttpContext.Session;
        var cart = session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        AddOrUpdateItem(cart, menuItem, model);
        session.SetObjectAsJson("Cart", cart);

        return new AddToCartResult { StatusMessage = "✅ Item added to cart!" };
    }

    public void AddOrUpdateItem(List<CartItem> cart, MenuItem menuItem, AddToCartViewModel model)
    {  // check if they item is already in the cart or not 
        var existingItem = cart.FirstOrDefault(i => i.MenuItemID == model.MenuItemID);

        if (existingItem != null)
        {
            existingItem.Quantity += model.Quantity; // modifies the quentity and reflects the changes to the cart
            if (!string.IsNullOrWhiteSpace(model.Note))
                existingItem.Note = model.Note; //override note or place it 
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
            Quantity = model.Quantity,
            Note = model.Note,
            RoutingTarget = menuItem.RoutingTarget.ToString()
        };
    }

    public List<CartItem> GetCartItems()
    {
        return _httpContextAccessor.HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

    }

    public void SaveCartItems(List<CartItem> items)
    {
        _httpContextAccessor.HttpContext.Session
            .SetObjectAsJson("Cart", items);
    }
    //to remove single item from the cart
    public void RemoveItem(int id)
    {
        var session = _httpContextAccessor.HttpContext.Session;
        var cart = session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        cart.RemoveAll(i => i.MenuItemID == id);
        session.SetObjectAsJson("Cart", cart);
    }
    // to delete the whole cart once
    public void ClearCart()
    {
        _httpContextAccessor.HttpContext.Session.Remove("Cart");
    }

    public CartViewModel BuildCartViewModel(List<RestaurantTable> tables)
    {
        // Get the items currently saved in the cart from session
        var itemsInCart = GetCartItems();

        // Convert each CartItem into a simpler view model for the page
        var viewModelItems = new List<CartItemViewModel>();

        foreach (var item in itemsInCart)
        {
            viewModelItems.Add(new CartItemViewModel
            {
                MenuItemID = item.MenuItemID,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                Note = item.Note,
                RoutingTarget = Enum.Parse<RoutingTarget>(item.RoutingTarget)

            });
        }

        // Create the full view model to return to the view wrapping eveything to Cartviewmodel
        var cartViewModel = new CartViewModel
        {
            Items = viewModelItems,
            Tables = tables,             // All available restaurant tables
            TipAmount = 0,               // Default tip
            Comment = "",                // Empty comment box
            SelectedTableID = 0          // No table selected yet
        };

        return cartViewModel;
    }


}


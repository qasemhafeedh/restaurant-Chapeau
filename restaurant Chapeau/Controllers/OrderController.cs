using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using restaurant_Chapeau.Helpers;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using restaurant_Chapeau.ViewModels;
using restaurant_Chapeau.Enums;

namespace restaurant_Chapeau.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMenuItemService _menuItemService;
        private readonly ITableService _tableService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IMenuItemService menuItemService,
            ITableService tableService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _menuItemService = menuItemService;
            _tableService = tableService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Menu()
        {
            try
            {
                var menuItems = await _menuItemService.GetAllMenuItemsAsync();
                return View(menuItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load menu.");
                return View("Error", "Unable to load the menu at this time.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartViewModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var menuItem = await _menuItemService.GetMenuItemByIdAsync(model.MenuItemID);

            if (menuItem == null || model.Quantity < 1 || model.Quantity > menuItem.QuantityAvailable)
            {
                TempData["OrderStatus"] = "⚠️ Invalid item or quantity.";
                return RedirectToAction("Menu");
            }

            var existingItem = cart.FirstOrDefault(i => i.MenuItemID == model.MenuItemID);
            if (existingItem != null)
            {
                existingItem.Quantity += model.Quantity;
                if (!string.IsNullOrEmpty(model.Note))
                    existingItem.Note = model.Note;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MenuItemID = menuItem.MenuItemID,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = model.Quantity,
                    Note = model.Note,
                    RoutingTarget = menuItem.RoutingTarget.ToString()
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["OrderStatus"] = "✅ Item added to cart!";
            return RedirectToAction("Menu");
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            try
            {
                var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
                var availableTables = await _tableService.GetAllTablesAsync();

                var viewModel = new CartViewModel
                {
                    Items = cartItems.Select(c => new CartItemViewModel
                    {
                        MenuItemID = c.MenuItemID,
                        Name = c.Name,
                        Price = c.Price,
                        Quantity = c.Quantity,
                        Note = c.Note,
                        RoutingTarget = Enum.Parse<RoutingTarget>(c.RoutingTarget)
                    }).ToList(),
                    Tables = availableTables,
                    TipAmount = 0,
                    Comment = "",
                    SelectedTableID = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart.");
                TempData["OrderStatus"] = "⚠️ Unable to load cart at this time.";
                return RedirectToAction("Menu");
            }
        }

        [HttpGet]
        public IActionResult RemoveCartItem(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            cart.RemoveAll(i => i.MenuItemID == id);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["OrderStatus"] = "❌ Item removed from cart.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["OrderStatus"] = "🗑️ Cart cleared.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(CartViewModel model)
        {
            try
            {
                if (model.Items == null || !model.Items.Any())
                {
                    TempData["OrderStatus"] = "⚠️ Cart is empty.";
                    return RedirectToAction(nameof(Cart));
                }

                if (model.SelectedTableID == 0)
                {
                    TempData["OrderStatus"] = "⚠️ Please select a table.";
                    return RedirectToAction(nameof(Cart));
                }

                var orderModel = new OrderSubmission
                {
                    TableID = model.SelectedTableID,
                    CartItems = model.Items.Select(i => new CartItem
                    {
                        MenuItemID = i.MenuItemID,
                        Name = i.Name,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        Note = i.Note,
                        RoutingTarget = i.RoutingTarget.ToString()
                    }).ToList(),
                    Comment = model.Comment,
                    TipAmount = model.TipAmount
                };

                int userId = HttpContext.Session.GetInt32("UserID") ?? 1;
                bool isSubmitted = await _orderService.SubmitOrderAsync(orderModel, userId);

                if (isSubmitted)
                {
                    HttpContext.Session.Remove("Cart");
                    TempData["OrderStatus"] = "✅ Order submitted successfully!";
                }
                else
                {
                    TempData["OrderStatus"] = "⚠️ Table is currently reserved.";
                }

                return RedirectToAction(nameof(Cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order submission failed.");
                TempData["OrderStatus"] = "❌ Unexpected error while submitting order.";
                return RedirectToAction(nameof(Cart));
            }
        }
    }
}

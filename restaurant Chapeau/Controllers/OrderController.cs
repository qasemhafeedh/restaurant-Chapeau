using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using restaurant_Chapeau.Helpers;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using restaurant_Chapeau.ViewModels;
using restaurant_Chapeau.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace restaurant_Chapeau.Controllers
{
    public class OrderController : Controller
    {   private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IMenuItemService _menuItemService;
        private readonly ITableService _tableService;
        private readonly ILogger<OrderController> _logger;

        public OrderController( IOrderService orderService,  IMenuItemService menuItemService, ITableService tableService, ILogger<OrderController> logger, ICartService cartService)
        {
            _orderService = orderService;
            _menuItemService = menuItemService;
            _tableService = tableService;
            _logger = logger;
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Menu()
        {
            try
            {
                List<MenuItem> menuItems = await _menuItemService.GetAllMenuItemsAsync();
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
            var result = await _cartService.AddToCartAsync(model);

            TempData["OrderStatus"] = result.StatusMessage;
            return RedirectToAction("Menu");
        }



        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            try
            {
                var tables = await _tableService.GetAllTablesAsync();
                var viewModel = _cartService.BuildCartViewModel(tables);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart.");
                TempData["OrderStatus"] = " Unable to load cart at this time.";
                return RedirectToAction("Menu");
            }
        }

        [HttpGet]
        public IActionResult RemoveCartItem(int id)
        {
            _cartService.RemoveItem(id);
            TempData["OrderStatus"] = " Item removed from cart.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            TempData["OrderStatus"] = " Cart cleared successfully.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(CartViewModel model)
        {
            try
            {
                var result = await _orderService.TrySubmitOrderAsync(model, GetUserId());

                TempData["OrderStatus"] = result.StatusMessage;

                if (result.Success)
                {
                    HttpContext.Session.Remove("Cart");
                }

                return RedirectToAction(nameof(Cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order submission failed.");
                TempData["OrderStatus"] = " Unexpected error while submitting order.";
                return RedirectToAction(nameof(Cart));
            }
        }

        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserID") ?? 1;
        }





    }
}

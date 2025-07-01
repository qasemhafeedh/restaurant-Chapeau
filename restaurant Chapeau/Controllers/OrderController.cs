using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using restaurant_Chapeau.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace restaurant_Chapeau.Controllers
{
    public class OrderController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IMenuItemService _menuItemService;
        private readonly ITableService _tableService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IMenuItemService menuItemService,
            ITableService tableService,
            ILogger<OrderController> logger,
            ICartService cartService)
        {
            _orderService = orderService;
            _menuItemService = menuItemService;
            _tableService = tableService;
            _logger = logger;
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> SelectTable()
        {
            try
            {
                List<RestaurantTable> tables = await _tableService.GetAllTablesAsync();
                return View(tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tables.");
                TempData["OrderStatus"] = " Unable to load table list.";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public IActionResult SelectTable(int tableId)
        {
            try
            {
                string message = _orderService.SetSelectedTableId(tableId);
                TempData["OrderStatus"] = message;
                return RedirectToAction(nameof(Menu));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting table.");
                TempData["OrderStatus"] = " Failed to select table.";
                return RedirectToAction(nameof(SelectTable));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Menu()
        {
            if (!_orderService.IsTableSelected())
                return RedirectToAction(nameof(SelectTable));

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
            

            try
            {
                model.TableID = _orderService.GetSelectedTableId();
                AddToCartResult result = await _cartService.AddToCartAsync(model);


                TempData["OrderStatus"] = result.StatusMessage;
                return RedirectToAction(nameof(Menu));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item to cart.");
                TempData["OrderStatus"] = " Unable to add item to cart.";
                return RedirectToAction(nameof(Menu));
            }
        }

        [HttpGet]
        public IActionResult Cart()
        {
            if (!_orderService.IsTableSelected())
                return RedirectToAction(nameof(SelectTable));

            try
            {
                CartViewModel viewModel = new CartViewModel
                {
                    Items = _cartService.BuildCartItemViewModels(),
                    SelectedTableID = _orderService.GetSelectedTableId()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart.");
                TempData["OrderStatus"] = " Unable to load cart at this time.";
                return RedirectToAction(nameof(Menu));
            }
        }



        [HttpGet]
        public IActionResult RemoveCartItem(int id)
        {
            try
            {
                _cartService.RemoveItem(id);
                TempData["OrderStatus"] = " Item removed from cart.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to remove cart item with ID {id}.");
                TempData["OrderStatus"] = " Could not remove item from cart.";
            }
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public IActionResult ClearCart()
        {
            try
            {
                _cartService.ClearCart();
                TempData["OrderStatus"] = " Cart cleared successfully.";
                return RedirectToAction(nameof(SelectTable));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear cart.");
                TempData["OrderStatus"] = " Could not clear the cart.";
            }
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(CartViewModel model)
        {
            if (!_orderService.IsTableSelected())
                return RedirectToAction(nameof(SelectTable));

            model.SelectedTableID = _orderService.GetSelectedTableId();

            try
            {
                SubmitOrderResult result = await _orderService.ProcessOrderSubmissionAsync(model, GetUserId());

                TempData["OrderStatus"] = result.StatusMessage;

                if (result.Success)
                    _orderService.ClearOrderSession();

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
            int? userId = HttpContext.Session.GetInt32("UserID");
            return userId ?? 1;
        }
    }
}

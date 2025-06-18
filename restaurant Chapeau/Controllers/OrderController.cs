using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Helpers;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMenuItemService _menuItemService;
        private readonly ITableService _tableService;

        public OrderController(
            IOrderService orderService,
            IMenuItemService menuItemService,
            ITableService tableService)
        {
            _orderService = orderService;
            _menuItemService = menuItemService;
            _tableService = tableService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var menuItems = await _menuItemService.GetAllMenuItemsAsync();
            return View(menuItems);
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new();
            var availableTables = await _tableService.GetAllTablesAsync();

            var viewModel = new OrderSubmission
            {
                CartItems = cartItems,
                Tables = availableTables
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(OrderSubmission model)
        {
            if (model.CartItems == null || !model.CartItems.Any())
            {
                TempData["OrderStatus"] = "❌ Cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            if (model.TableID == 0)
            {
                TempData["OrderStatus"] = "❌ You must select a table before submitting the order.";
                return RedirectToAction(nameof(Cart));
            }

            int userId = HttpContext.Session.GetInt32("UserID") ?? 1;

            bool isSubmitted = await _orderService.SubmitOrderAsync(model, userId);

            TempData["OrderStatus"] = isSubmitted
                ? "✅ Order submitted successfully!"
                : "❌ Table is currently reserved.";

            return RedirectToAction(nameof(Cart));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Helpers;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> Menu()
        {
            try
            {
                var menu = await _orderService.GetMenuItemsAsync();
                return View(menu);
            }
            catch (Exception ex)
            {
                TempData["OrderStatus"] = $"Error loading menu: {ex.Message}";
                return RedirectToAction("Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            try
            {
                var model = new OrderSubmission
                {
                    CartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new(),
                    Tables = await _orderService.GetTablesAsync()
                };

                ViewBag.Tables = model.Tables;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["OrderStatus"] = $"Error loading cart: {ex.Message}";
                return View(new OrderSubmission());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(OrderSubmission model)
        {
            if (model.CartItems == null || !model.CartItems.Any())
            {
                TempData["OrderStatus"] = "Cart is empty.";
                return RedirectToAction("Cart");
            }
            if (model.TableID == 0)
            {
                TempData["OrderStatus"] = "❌ You must select a table before submitting the order.";
                return RedirectToAction("Cart");
            }


            try
            {
                int userId = HttpContext.Session.GetInt32("UserID") ?? 1;
                bool success = await _orderService.SubmitOrderAsync(model, userId);

                TempData["OrderStatus"] = success
                    ? "✅ Order submitted successfully!"
                    : "❌ Table is currently reserved.";

                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                TempData["OrderStatus"] = $"❌ Error: {ex.Message}";
                return RedirectToAction("Cart");
            }
        }
    }
}

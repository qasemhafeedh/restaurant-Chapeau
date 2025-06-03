using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Controllers
{
    public class OrderManagementController : Controller
    {
        private readonly IOrderManagementService _orderManagementService;

        public OrderManagementController(IOrderManagementService orderManagementService)
        {
            _orderManagementService = orderManagementService;
        }

        public IActionResult Orders()
        {
            var allOrders = _orderManagementService.GetAllOrders();
            var runningOrders = _orderManagementService.GetRunningOrders(allOrders);
            return View(runningOrders); // Expects Views/OrderManagement/Orders.cshtml
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            var order = _orderManagementService.GetOrderById(id);
            if (order == null)
                return NotFound();
            return View(order); // Expects Views/OrderManagement/OrderDetails.cshtml
        }

        [HttpPost]
        public IActionResult UpdateOrderItemStatus(int itemId, ItemStatus newStatus, int orderId)
        {
            _orderManagementService.UpdateOrderItemStatus(itemId, newStatus);
            return RedirectToAction("OrderDetails", new { id = orderId });
        }
    }
}

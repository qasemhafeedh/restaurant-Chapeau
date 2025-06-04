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

        // Display running orders (default view)
        public IActionResult Orders(string orderType = "running")
        {
            var allOrders = _orderManagementService.GetAllOrders();
            List<Orders> orders;

            if (orderType.ToLower() == "finished")
            {
                orders = _orderManagementService.GetFinishedOrders(allOrders);
            }
            else
            {
                orders = _orderManagementService.GetRunningOrders(allOrders);
            }

            ViewBag.OrderType = orderType;
            return View(orders);
        }

        // Kitchen view - shows only food orders grouped by course
        public IActionResult KitchenOrders()
        {
            var allOrders = _orderManagementService.GetAllOrders();
            var runningOrders = _orderManagementService.GetRunningOrders(allOrders);

            // Filter for food orders only (exclude drinks)
            var foodOrders = runningOrders.Select(order => new Orders
            {
                Id = order.Id,
                TableNumber = order.TableNumber,
                OrderTime = order.OrderTime,
                comment = order.comment,
                Status = order.Status,
                Items = order.Items.Where(item => item.courseType != CourseType.Drink).ToList()
            }).Where(order => order.Items.Any()).ToList();

            return View(foodOrders);
        }

        // Bar view - shows only drink orders
        public IActionResult BarOrders()
        {
            var allOrders = _orderManagementService.GetAllOrders();
            var runningOrders = _orderManagementService.GetRunningOrders(allOrders);

            // Filter for drink orders only
            var drinkOrders = runningOrders.Select(order => new Orders
            {
                Id = order.Id,
                TableNumber = order.TableNumber,
                OrderTime = order.OrderTime,
                comment = order.comment,
                Status = order.Status,
                Items = order.Items.Where(item => item.courseType == CourseType.Drink).ToList()
            }).Where(order => order.Items.Any()).ToList();

            return View(drinkOrders);
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            var order = _orderManagementService.GetOrderById(id);
            if (order == null)
                return NotFound();
            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateOrderItemStatus(int itemId, ItemStatus newStatus, int orderId, string returnAction = "Orders")
        {
            _orderManagementService.UpdateOrderItemStatus(itemId, newStatus);

            // Redirect back to the appropriate view
            if (returnAction == "KitchenOrders")
                return RedirectToAction("KitchenOrders");
            else if (returnAction == "BarOrders")
                return RedirectToAction("BarOrders");
            else if (returnAction == "OrderDetails")
                return RedirectToAction("OrderDetails", new { id = orderId });
            else
                return RedirectToAction("Orders");
        }

        [HttpPost]
        public IActionResult UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus, string returnAction = "Orders")
        {
            _orderManagementService.UpdateCourseStatus(orderId, courseType, newStatus);

            // Redirect back to the appropriate view
            if (returnAction == "KitchenOrders")
                return RedirectToAction("KitchenOrders");
            else if (returnAction == "BarOrders")
                return RedirectToAction("BarOrders");
            else
                return RedirectToAction("Orders");
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, OrderStatus newStatus, string returnAction = "Orders")
        {
            _orderManagementService.UpdateOrderStatus(orderId, newStatus);

            // Redirect back to the appropriate view
            if (returnAction == "KitchenOrders")
                return RedirectToAction("KitchenOrders");
            else if (returnAction == "BarOrders")
                return RedirectToAction("BarOrders");
            else
                return RedirectToAction("Orders");
        }
    }
}
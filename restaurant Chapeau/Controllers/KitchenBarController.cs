using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Controllers
{
    public class KitchenBarController : Controller
    {
        private readonly IOrderService _orderService;

        public KitchenBarController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult KitchenRunningOrders()
        {
            var runningOrders = _orderService.GetRunningOrders();
            var kitchenOrders = _orderService.FilterOrdersByTarget(runningOrders, Order.RoutingTarget.Kitchen);
            return View("RunningOrders", kitchenOrders);
        }

        public IActionResult KitchenFinishedOrders()
        {
            var finishedOrders = _orderService.GetFinishedOrders();
            var kitchenOrders = _orderService.FilterOrdersByTarget(finishedOrders, Order.RoutingTarget.Kitchen);
            return View("FinishedOrders", kitchenOrders);
        }

        public IActionResult BarRunningOrders()
        {
            var runningOrders = _orderService.GetRunningOrders();
            var barOrders = _orderService.FilterOrdersByTarget(runningOrders, Order.RoutingTarget.Bar);
            return View("RunningOrders", barOrders);
        }

        public IActionResult BarFinishedOrders()
        {
            var finishedOrders = _orderService.GetFinishedOrders();
            var barOrders = _orderService.FilterOrdersByTarget(finishedOrders, Order.RoutingTarget.Bar);
            return View("FinishedOrders", barOrders);
        }


        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateKitchenOrderItemStatus(int itemId, ItemStatus newStatus, int orderId)
        {
            _orderService.UpdateOrderItemStatus(itemId, newStatus);

            // Redirect back to running orders instead of order details
            return RedirectToAction("RunningOrders");
        }

        [HttpPost]
        public IActionResult UpdateKitchenCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus)
        {
            _orderService.UpdateCourseStatus(orderId, courseType, newStatus);

            return RedirectToAction("RunningOrders");
        }
        [HttpPost]
        public IActionResult UpdateBarOrderItemStatus(int itemId, ItemStatus newStatus, int orderId)
        {
            _orderService.UpdateOrderItemStatus(itemId, newStatus);

            // Redirect back to running orders instead of order details
            return RedirectToAction("RunningOrders");
        }


    }
}
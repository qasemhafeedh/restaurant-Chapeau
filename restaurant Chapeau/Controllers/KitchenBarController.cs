using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Controllers
{
    public class KitchenBarController : Controller
    {
        private readonly IKitchenBarService _kitchenBarService;

        public KitchenBarController(IKitchenBarService kitchenBarService)
        {
            _kitchenBarService = kitchenBarService;
        }

        public IActionResult KitchenRunningOrders()
        {
            var runningOrders = _kitchenBarService.GetRunningOrders();
            return View("Kitchen/RunningOrders", runningOrders);
        }
        public IActionResult KitchenFinishedOrders()
        {
            var finisedOrders = _kitchenBarService.GetFinishedOrders();
            return View("Kitchen/FinishedOrders", finisedOrders);
        }
        public IActionResult BarFinishedOrders()
        {
            var finisedOrders = _kitchenBarService.GetFinishedOrders();
            return View("Bar/FinishedOrders", finisedOrders);
        }
        public IActionResult BarRunningOrders()
        {
            var runningOrders = _kitchenBarService.GetRunningOrders();
            return View("Bar/RunningOrders", runningOrders);
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            var order = _kitchenBarService.GetOrderById(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateKitchenOrderItemStatus(int itemId, ItemStatus newStatus, int orderId)
        {
            _kitchenBarService.UpdateOrderItemStatus(itemId, newStatus);

            // Redirect back to running orders instead of order details
            return RedirectToAction("KitchenRunningOrders");
        }

        [HttpPost]
        public IActionResult UpdateKitchenCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus)
        {
            _kitchenBarService.UpdateCourseStatus(orderId, courseType, newStatus);

            return RedirectToAction("KitchenRunningOrders");
        }
        [HttpPost]
        public IActionResult UpdateBarOrderItemStatus(int itemId, ItemStatus newStatus, int orderId)
        {
            _kitchenBarService.UpdateOrderItemStatus(itemId, newStatus);

            // Redirect back to running orders instead of order details
            return RedirectToAction("BarRunningOrders");
        }


    }
}
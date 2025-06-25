using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using System;

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
            try
            {
                var runningOrders = _kitchenBarService.GetRunningOrders();
                var kitchenOrders = _kitchenBarService.FilterOrdersByTarget(runningOrders, Order.RoutingTarget.Kitchen);
                return View("RunningOrders", kitchenOrders);
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public IActionResult KitchenFinishedOrders()
        {
            try
            {
                var finishedOrders = _kitchenBarService.GetFinishedOrders();
                var kitchenOrders = _kitchenBarService.FilterOrdersByTarget(finishedOrders, Order.RoutingTarget.Kitchen);
                return View("FinishedOrders", kitchenOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public IActionResult BarRunningOrders()
        {
            try
            {
                var runningOrders = _kitchenBarService.GetRunningOrders();
                var barOrders = _kitchenBarService.FilterOrdersByTarget(runningOrders, Order.RoutingTarget.Bar);
                return View("RunningOrders", barOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public IActionResult BarFinishedOrders()
        {
            try
            {
                var finishedOrders = _kitchenBarService.GetFinishedOrders();
                var barOrders = _kitchenBarService.FilterOrdersByTarget(finishedOrders, Order.RoutingTarget.Bar);
                return View("FinishedOrders", barOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            try
            {
                var order = _kitchenBarService.GetOrderById(id);
                if (order == null)
                    return NotFound();

                return View(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult UpdateKitchenOrderItemStatus(int itemId, ItemStatus newStatus, int orderId)
        {
            try
            {
                _kitchenBarService.UpdateOrderItemStatus(itemId, newStatus);
                return RedirectToAction("KitchenRunningOrders");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult UpdateKitchenCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus)
        {
            try
            {
                _kitchenBarService.UpdateCourseStatus(orderId, courseType, newStatus);
                return RedirectToAction("KitchenRunningOrders");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost]
        public IActionResult UpdateOrderStatus(Order order, OrderStatus newStatus)
        {
            try
            {
                _kitchenBarService.UpdeteOrderStatus(order, newStatus);
                return RedirectToAction("KitchenRunningOrders");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult UpdateBarOrderItemStatus(int itemId, ItemStatus newStatus, int orderId)
        {
            try
            {
                _kitchenBarService.UpdateOrderItemStatus(itemId, newStatus);
                return RedirectToAction("BarRunningOrders");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

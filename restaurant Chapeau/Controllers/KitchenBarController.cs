using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using System;

namespace restaurant_Chapeau.Controllers
{
    public class KitchenBarController : Controller
    {
        private readonly IOrderService _orderService;

        public KitchenBarController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public List<Order> GetAllOrders(bool isReady)
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");
                bool isKitchen = role == "Kitchen";

                return _orderService.GetAllOrders(isKitchen, isReady);
            }
            catch (Exception)
            {
                // Optional: log exception
                return new List<Order>();
            }
        }

        public IActionResult RunningOrders()
        {
            var orders = GetAllOrders(true);
            return View(orders);
        }

        public IActionResult FinishedOrders()
        {
            var orders = GetAllOrders(false);
            return View(orders);
        }


 

        [HttpPost]
        public IActionResult UpdateOrderItemStatus(int itemId, ItemStatus newStatus, int orderId)
        {
            try
            {
                _orderService.UpdateOrderItemStatus(orderId, itemId, newStatus);
                return RedirectToAction("RunningOrders");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus)
        {
            try
            {
                _orderService.UpdateCourseStatus(orderId, courseType, newStatus);
                return RedirectToAction("RunningOrders");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            try
            {
                // Get the order first, then update its status
                var order = _orderService.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                _orderService.UpdeteOrderStatus(order, newStatus);
                return RedirectToAction("RunningOrders");
            }
            catch (Exception ex)
            {
                // Add logging here if needed
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}

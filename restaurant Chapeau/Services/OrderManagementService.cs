using System.Collections.Generic;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;

namespace restaurant_Chapeau.Services.Interfaces
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly IOrderManagement _repo;

        public OrderManagementService(IOrderManagement repo)
        {
            _repo = repo;
        }

        public List<Orders> GetAllOrders()
        {
            return _repo.GetAllOrders();
        }

        public List<Orders> GetRunningOrders(List<Orders> orders)
        {
            return _repo.GetRunningOrders(orders);
        }

        public List<Orders> GetFinishedOrders(List<Orders> orders)
        {
            return _repo.GetFinishedOrders(orders);
        }

        public Orders GetOrderById(int orderId)
        {
            return _repo.GetOrderById(orderId);
        }

        public void UpdateOrderItemStatus(int itemId, ItemStatus newStatus)
        {
            _repo.UpdateOrderItemStatus(itemId, newStatus);
        }

        public void UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus)
        {
            _repo.UpdateCourseStatus(orderId, courseType, newStatus);
        }

        public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            _repo.UpdateOrderStatus(orderId, newStatus);
        }
    }
}
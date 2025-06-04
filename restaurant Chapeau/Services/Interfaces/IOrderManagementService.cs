using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IOrderManagementService
    {
        List<Orders> GetAllOrders();
        List<Orders> GetRunningOrders(List<Orders> orders);
        List<Orders> GetFinishedOrders(List<Orders> orders);
        Orders GetOrderById(int orderId);
        void UpdateOrderItemStatus(int itemId, ItemStatus newStatus);
        void UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus);
        void UpdateOrderStatus(int orderId, OrderStatus newStatus);
    }
}
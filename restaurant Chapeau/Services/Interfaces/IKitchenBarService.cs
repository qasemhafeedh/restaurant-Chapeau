using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IKitchenBarService
    {
        List<Order> GetRunningOrders();
        List<Order> GetFinishedOrders();
        Order GetOrderById(int orderId);
        void UpdateOrderItemStatus(int orderId, ItemStatus newStatus);
        void UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus);


    }
}

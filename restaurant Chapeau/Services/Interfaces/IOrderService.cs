using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Services.Interfaces
{
    
    
    public interface IOrderService
    {
        Task<bool> SubmitOrderAsync(OrderSubmission model, int userId);
        List<Order> GetRunningOrders();
        List<Order> GetFinishedOrders();
        Order GetOrderById(int orderId);
        void UpdateOrderItemStatus(int orderItemId, ItemStatus newStatus);
        void UpdeteOrderStatus(Order order, OrderStatus newStatus);
        void UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus);
        List<Order> FilterOrdersByTarget(List<Order> orders, Order.RoutingTarget target);
    }


}

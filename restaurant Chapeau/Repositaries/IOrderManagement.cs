using restaurant_Chapeau.Models;
using static restaurant_Chapeau.Models.OrderManagement;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IOrderManagement
    {
        List<Orders> GetAllOrders();
        List<Orders> GetRunningOrders(List<Orders> orders);
        Orders GetOrderById(int orderId);
        void UpdateOrderItemStatus(int orderId, OrderStatus newStatus);
    }
}

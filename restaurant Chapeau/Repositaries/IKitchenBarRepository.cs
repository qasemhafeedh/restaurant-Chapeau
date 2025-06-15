using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IKitchenBarRepository
    {
        List<Order> GetRunningOrders();
        List<Order> GetFinishedOrders();

        Order GetOrderById(int orderId);
        void UpdateOrderItemStatus(int orderItemId, ItemStatus newStatus);
    }
}

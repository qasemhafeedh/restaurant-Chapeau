using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IOrderManagementService
    {
        List<Orders> GetAllOrders();
        List<Orders> GetRunningOrders(List<Orders>orders);
        Orders GetOrderById(int orderId);
        void UpdateOrderItemStatus(int orderId, ItemStatus newStatus);

    }
}

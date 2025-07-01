using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositaries
{
    
    using restaurant_Chapeau.Models;

    public interface IOrderRepository
    {
        Task<int> CreateOrderAsync(OrderSubmission model, int userId);
        Task AddOrderItemsAsync(int orderId, List<CartItem> items);
        /// //////////////////////////////(below this is for bBr and Kitchen )///////////////////////////////////
        void UpdateOrderItemStatus(int orderId, int orderItemId, ItemStatus newStatus);
        List<Order> GetAllOrders(bool isKitchen, bool isReady);
        void UpdateOrderStatus(int orderId, OrderStatus newStatus);
        Order GetOrderById(int id);
    }

}

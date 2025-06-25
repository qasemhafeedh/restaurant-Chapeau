using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositaries
{
    
    using restaurant_Chapeau.Models;

    public interface IOrderRepository
    {
        Task<int> CreateOrderAsync(OrderSubmission model, int userId);
        Task AddOrderItemsAsync(int orderId, List<CartItem> items);
        /// //////////////////////////////(below this is for bBr and Kitchen )///////////////////////////////////
        void UpdateOrderItemStatus(int orderItemId, ItemStatus newStatus);
        List<Order> GetAllOrders();
    }

}

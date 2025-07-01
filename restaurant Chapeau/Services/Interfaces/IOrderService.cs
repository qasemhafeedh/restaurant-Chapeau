using restaurant_Chapeau.Models;
using restaurant_Chapeau.ViewModels;

namespace restaurant_Chapeau.Services.Interfaces
{
    
    
    public interface IOrderService
    {
        Task<bool> SubmitOrderAsync(OrderSubmission model, int userId);
        Task<SubmitOrderResult> ProcessOrderSubmissionAsync(CartViewModel model, int userId);

        public bool IsTableSelected();


        public int GetSelectedTableId();


        public string SetSelectedTableId(int tableId);


        public void ClearOrderSession();

        /////////////////////////////(Below this is for Kitchen And Bar)////////////////////////////////
        List<Order> GetAllOrders(bool isKitchen, bool isReady);
        void UpdateOrderItemStatus(int orderId, int orderItemId, ItemStatus newStatus);
        void UpdeteOrderStatus(Order order, OrderStatus newStatus);
        void UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus);
        Order GetOrderById(int id);

    }


}

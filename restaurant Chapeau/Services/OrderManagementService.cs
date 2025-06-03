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
        public List<Orders> GetRunningOrders(List<Orders> o)
        {
            return _repo.GetRunningOrders(o);

        }
        public Orders GetOrderById(int orderId)
        {
            return _repo.GetOrderById(orderId);
        }
        public void UpdateOrderItemStatus(int orderId, ItemStatus newStatus)
        {
            return;
        }
        
    }
}

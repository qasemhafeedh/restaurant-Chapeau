using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MenuItem>> GetMenuItemsAsync()
        {
            return await _repository.GetMenuItemsAsync();
        }

        public async Task<List<RestaurantTable>> GetTablesAsync()
        {
            return await _repository.GetTablesAsync();
        }

        public async Task<bool> SubmitOrderAsync(OrderSubmission model, int userId)
        {
            return await _repository.SubmitOrderAsync(model, userId);
        }

    }
}

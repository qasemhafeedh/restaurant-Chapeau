using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services.Interfaces
{

    public class OrderService : IOrderService
    {
        private readonly ITableService _tableService;
        private readonly IMenuItemService _menuItemService;
        private readonly IOrderRepository _orderRepository;

        public OrderService(
            ITableService tableService,
            IMenuItemService menuItemService,
            IOrderRepository orderRepository)
        {
            _tableService = tableService;
            _menuItemService = menuItemService;
            _orderRepository = orderRepository;
        }

        public async Task<bool> SubmitOrderAsync(OrderSubmission model, int userId)
        {
            if (await _tableService.IsReservedAsync(model.TableID))
                return false;

            foreach (var item in model.CartItems)
            {
                var stockOk = await _menuItemService.IsStockAvailableAsync(item.MenuItemID, item.Quantity);
                if (!stockOk)
                    throw new Exception($"Insufficient stock for {item.Name}");

                await _menuItemService.DecreaseStockAsync(item.MenuItemID, item.Quantity);
            }

            await _tableService.ReserveAsync(model.TableID);

            int orderId = await _orderRepository.CreateOrderAsync(model, userId);
            await _orderRepository.AddOrderItemsAsync(orderId, model.CartItems);

            return true;
        }
    }
}


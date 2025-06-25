using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    /// <summary>
    /// Handles business logic related to orders.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ITableService _tableService;
        private readonly IMenuItemService _menuItemService;
        private readonly IOrderRepository _orderRepository;


        /// <summary>
        /// Constructor that injects dependencies for table, menu item, and order services.
        /// </summary>
        /// <param name="tableService">Service to handle table reservation logic.</param>
        /// <param name="menuItemService">Service to handle menu item stock and retrieval.</param>
        /// <param name="orderRepository">Repository for order database operations.</param>
        public OrderService(
            ITableService tableService,
            IMenuItemService menuItemService,
            IOrderRepository orderRepository)
        {
            _tableService = tableService;
            _menuItemService = menuItemService;
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Submits a new order after validating table availability and item stock.
        /// </summary>
        /// <param name="model">The order submission data including cart and table info.</param>
        /// <param name="userId">The user ID submitting the order.</param>
        /// <returns>True if order was successfully submitted, otherwise false.</returns>
        public async Task<bool> SubmitOrderAsync(OrderSubmission model, int userId)
        {
            // Check if table is already reserved
            if (await _tableService.IsReservedAsync(model.TableID))
                return false;

            // Verify stock availability and update stock
            foreach (var item in model.CartItems)
            {
                var stockOk = await _menuItemService.IsStockAvailableAsync(item.MenuItemID, item.Quantity);
                if (!stockOk)
                    throw new Exception($"Insufficient stock for {item.Name}");

                await _menuItemService.DecreaseStockAsync(item.MenuItemID, item.Quantity);
            }

            // Reserve table
            await _tableService.ReserveAsync(model.TableID);

            // Create order and add items to the database
            int orderId = await _orderRepository.CreateOrderAsync(model, userId);
            await _orderRepository.AddOrderItemsAsync(orderId, model.CartItems);

            return true;
        }
       
    }
}

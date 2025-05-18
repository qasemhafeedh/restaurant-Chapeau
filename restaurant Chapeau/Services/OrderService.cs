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
            // 1. Check if table is available
            if (await _repository.IsTableReservedAsync(model.TableID))
                return false;

            // 2. Check stock for each item
            foreach (var item in model.CartItems)
            {
                if (!await _repository.IsStockAvailableAsync(item.MenuItemID, item.Quantity))
                    return false;
            }

            // 3. Decrease stock
            foreach (var item in model.CartItems)
            {
                await _repository.DecreaseStockAsync(item.MenuItemID, item.Quantity);
            }

            // 4. Create order
            int orderId = await _repository.CreateOrderAsync(model.TableID, model.Comment);

            // 5. Add order items
            await _repository.AddOrderItemsAsync(orderId, model.CartItems);

            // 6. Reserve table
            await _repository.ReserveTableAsync(model.TableID);

            // 7. Calculate total + VAT
            decimal total = model.CartItems.Sum(i => i.Price * i.Quantity);
            decimal vat = model.CartItems.Sum(i => i.Price * i.Quantity * i.VATRate);

            // 8. Create invoice
            await _repository.CreateInvoiceAsync(orderId, userId, total, model.TipAmount, vat);

            return true;
        }
    }
}

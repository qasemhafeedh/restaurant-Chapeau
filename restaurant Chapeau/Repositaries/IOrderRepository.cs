using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositaries
{
    public interface IOrderRepository
    {
        Task<List<MenuItem>> GetMenuItemsAsync();
        Task<List<RestaurantTable>> GetTablesAsync();
        Task<bool> IsTableReservedAsync(int tableId);
        Task<bool> IsStockAvailableAsync(int menuItemId, int quantity);
        Task DecreaseStockAsync(int menuItemId, int quantity);
        Task<int> CreateOrderAsync(int tableId, string comment);
        Task AddOrderItemsAsync(int orderId, List<CartItem> items);
        Task CreateInvoiceAsync(int orderId, int userId, decimal total, decimal tip, decimal vat);
        Task ReserveTableAsync(int tableId);
        Task<bool> SubmitOrderAsync(OrderSubmission model, int userId);
    }
}

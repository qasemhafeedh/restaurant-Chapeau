using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositories
{
    public interface IPaymentRepository
    {
        Task<TableOrderView> GetCompleteOrderByTableIdAsync(int tableId);
        Task<int> GetOpenOrderIdByTableAsync(int tableId);
        Task<decimal> CalculateVATForOrderAsync(int orderId);
        Task CreateInvoiceAsync(Invoice invoice);
        Task SaveSplitPaymentAsync(int orderId, decimal amount, string method, string feedback);
        Task FreeTableAsync(int tableId);
        Task MarkOrderAsPaidAsync(int orderId);
        Task<List<RestaurantTable>> GetTablesWithUnpaidOrdersAsync();
    }
}

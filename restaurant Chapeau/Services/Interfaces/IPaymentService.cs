using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<TableOrderView> GetCompleteOrderByTableIdAsync(int tableId);
        Task<int> GetOpenOrderIdByTableAsync(int tableId);
        Task<decimal> GetTotalAmountForTableAsync(int tableId);
        Task<decimal> CalculateVATForOrderAsync(int orderId);
        Task SaveSplitPaymentAsync(int orderId, decimal amount, string method, string feedback);
        Task FreeTableAsync(int tableId);
        Task<List<RestaurantTable>> GetTablesWithUnpaidOrdersAsync();

        // ✅ Update this:
        Task CreateInvoiceAsync(Invoice invoice);
        Task MarkOrderAsPaidAsync(int orderId);
    }
}


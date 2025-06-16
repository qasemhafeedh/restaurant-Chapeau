using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;

        public PaymentService(IPaymentRepository repository)
        {
            _repository = repository;
        }

        public async Task<TableOrderView> GetCompleteOrderByTableIdAsync(int tableId)
        {
            return await _repository.GetCompleteOrderByTableIdAsync(tableId);
        }

        public async Task<int> GetOpenOrderIdByTableAsync(int tableId)
        {
            return await _repository.GetOpenOrderIdByTableAsync(tableId);
        }

        public async Task<decimal> CalculateVATForOrderAsync(int orderId)
        {
            return await _repository.CalculateVATForOrderAsync(orderId);
        }

        public async Task CreateInvoiceAsync(Invoice invoice)
        {
            await _repository.CreateInvoiceAsync(invoice);
        }

        public async Task SaveSplitPaymentAsync(int orderId, decimal amount, string method, string feedback)
        {
            await _repository.SaveSplitPaymentAsync(orderId, amount, method, feedback);
        }

        public async Task FreeTableAsync(int tableId)
        {
            await _repository.FreeTableAsync(tableId);
        }

        public async Task<List<RestaurantTable>> GetTablesWithUnpaidOrdersAsync()
        {
            return await _repository.GetTablesWithUnpaidOrdersAsync();
        }

        // ✅ FIXED: Actually marks the order as paid
        public async Task MarkOrderAsPaidAsync(int orderId)
        {
            await _repository.MarkOrderAsPaidAsync(orderId);
        }

        public async Task<decimal> GetTotalAmountForTableAsync(int tableId)
        {
            var order = await GetCompleteOrderByTableIdAsync(tableId);
            return order.TotalAmount + order.TotalLowVAT + order.TotalHighVAT;
        }
    }
}

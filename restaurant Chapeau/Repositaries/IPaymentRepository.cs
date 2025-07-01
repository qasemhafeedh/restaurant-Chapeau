// IPaymentRepository.cs (Synchronous Version)
using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Repositories
{
    public interface IPaymentRepository
    {
        TableOrderView GetCompleteOrderByTableId(int tableId);
        int GetOpenOrderIdByTable(int tableId);
        decimal CalculateVATForOrder(int orderId);
        void CreateInvoice(Invoice invoice);
        void SaveSplitPayment(int orderId, decimal amount, string method, string feedback);
        void FreeTable(int tableId);
        void MarkOrderAsPaid(int orderId);
        List<RestaurantTable> GetTablesWithUnpaidOrders();
        decimal GetTotalAmountForOrder(int orderId);

    }
}

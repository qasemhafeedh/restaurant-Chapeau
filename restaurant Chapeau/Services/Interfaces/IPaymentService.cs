// IPaymentService.cs (Synchronous Version)
using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IPaymentService
    {
        // ✅ For listing tables that still need to pay
        List<RestaurantTable> GetTablesWithUnpaidOrders();

        // ✅ To show the full order before payment
        TableOrderView GetCompleteOrderByTableId(int tableId);

        // ✅ For Finish Order screen
        FinishOrderViewModel BuildFinishOrderViewModel(int tableId);

        // ✅ To confirm and finalize an order payment
        (bool IsSuccess, string ErrorMessage) FinalizeOrder(FinishOrderViewModel model);

        // ✅ For displaying dynamic split payment UI
        SplitPaymentViewModel BuildSplitPaymentViewModel(int tableId, int numberOfGuests);

        // ✅ To process split payments from multiple people
        (bool IsSuccess, string ErrorMessage) ProcessSplitPayment(SplitPaymentViewModel model);
    }
}


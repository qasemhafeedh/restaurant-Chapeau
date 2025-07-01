using System.Collections.Generic;
using System.Linq;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;

        public PaymentService(IPaymentRepository repo)
        {
            _repo = repo;
        }

        public List<RestaurantTable> GetTablesWithUnpaidOrders()
        {
            return _repo.GetTablesWithUnpaidOrders();
        }

        public TableOrderView GetCompleteOrderByTableId(int tableId)
        {
            return _repo.GetCompleteOrderByTableId(tableId);
        }

        public FinishOrderViewModel BuildFinishOrderViewModel(int tableId)
        {
            var order = _repo.GetCompleteOrderByTableId(tableId);
            var vat = _repo.CalculateVATForOrder(order.OrderID);

            return new FinishOrderViewModel
            {
                TableID = tableId,
                OrderID = order.OrderID,
                TotalAmount = order.TotalAmount + order.TotalLowVAT + order.TotalHighVAT,
                VATAmount = vat
            };
        }

        public (bool IsSuccess, string ErrorMessage) FinalizeOrder(FinishOrderViewModel model)
        {
            var invoice = new Invoice
            {
                InvoiceNumber = System.Guid.NewGuid().ToString(),
                OrderID = model.OrderID,
                UserID = 1,
                TotalAmount = model.TotalAmount,
                TipAmount = model.TipAmount,
                VATAmount = model.VATAmount
            };

            _repo.CreateInvoice(invoice);
            _repo.FreeTable(model.TableID);
            return (true, "");
        }

        public SplitPaymentViewModel BuildSplitPaymentViewModel(int tableId, int numberOfGuests)
        {
            int orderId = _repo.GetOpenOrderIdByTable(tableId);
            if (orderId <= 0)
                return null;

            decimal totalAmount = _repo.GetTotalAmountForOrder(orderId); // ✅ Get total from repository

            return new SplitPaymentViewModel
            {
                TableID = tableId,
                OrderID = orderId,
                TotalAmount = totalAmount, // ✅ Set total for split calculation
                NumberOfGuests = numberOfGuests,
                Payments = Enumerable.Range(0, numberOfGuests)
                                     .Select(_ => new GuestInvoice())
                                     .ToList()
            };
        }




        public (bool IsSuccess, string ErrorMessage) ProcessSplitPayment(SplitPaymentViewModel model)
        {
            // 1. Total up what was paid
            var totalPaid = model.Payments.Sum(p => Math.Round(p.AmountPaid, 2));
            var expected = Math.Round(model.TotalAmount, 2);

            if (totalPaid != expected)
            {
                return (false, $"❌ Total paid (€{totalPaid:0.00}) does not match the expected amount (€{expected:0.00}).");
            }

            // 2. Create a separate invoice for each guest
            foreach (var p in model.Payments)
            {
                var invoice = new Invoice
                {
                    InvoiceNumber = Guid.NewGuid().ToString(),
                    OrderID = model.OrderID,
                    UserID = 1, // Later: pull this from session or controller context
                    TotalAmount = Math.Round(p.AmountPaid, 2),
                    TipAmount = 0,
                    VATAmount = 0,
                    CreatedAt = DateTime.Now
                };

                _repo.CreateInvoice(invoice);
            }

            // 3. Update order + table
            _repo.MarkOrderAsPaid(model.OrderID);
            _repo.FreeTable(model.TableID);

            return (true, "");
        }
    }
}

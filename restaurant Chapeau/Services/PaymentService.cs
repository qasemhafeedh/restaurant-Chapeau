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
            if (order == null || order.OrderID == 0)
                return null;

            var vat = _repo.CalculateVATForOrder(order.OrderID);
            var totalWithVAT = order.TotalAmount + order.TotalLowVAT + order.TotalHighVAT;

            return new FinishOrderViewModel
            {
                TableID = tableId,
                OrderID = order.OrderID,
                TotalAmount = totalWithVAT,
                VATAmount = vat,
                TipAmount = 0,
                PaymentMethod = "Cash",
                Feedback = ""
            };
        }

        public (bool IsSuccess, string ErrorMessage) FinalizeOrder(FinishOrderViewModel model)
        {
            try
            {
                // Calculate VAT properly
                var vatAmount = _repo.CalculateVATForOrder(model.OrderID);

                var invoice = new Invoice
                {
                    InvoiceNumber = System.Guid.NewGuid().ToString().Substring(0, 8),
                    OrderID = model.OrderID,
                    UserID = 1, // TODO: Get from session
                    TotalAmount = model.TotalAmount + model.TipAmount,
                    TipAmount = model.TipAmount,
                    VATAmount = vatAmount
                };

                _repo.CreateInvoice(invoice);
                _repo.MarkOrderAsPaid(model.OrderID);
                _repo.FreeTable(model.TableID);

                return (true, "Order completed successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error processing payment: {ex.Message}");
            }
        }

        public SplitPaymentViewModel BuildSplitPaymentViewModel(int tableId, int numberOfGuests)
        {
            int orderId = _repo.GetOpenOrderIdByTable(tableId);
            if (orderId <= 0)
                return null;

            var order = _repo.GetCompleteOrderByTableId(tableId);
            if (order == null)
                return null;

            // Calculate total including VAT
            decimal totalAmount = order.TotalAmount + order.TotalLowVAT + order.TotalHighVAT;

            // Ensure we have a valid total amount
            if (totalAmount <= 0)
            {
                totalAmount = _repo.GetTotalAmountForOrder(orderId);
            }

            // Ensure number of guests is valid
            if (numberOfGuests <= 0)
                numberOfGuests = 2;

            decimal amountPerPerson = totalAmount > 0 ? Math.Round(totalAmount / numberOfGuests, 2) : 0;

            return new SplitPaymentViewModel
            {
                TableID = tableId,
                OrderID = orderId,
                TotalAmount = totalAmount,
                NumberOfGuests = numberOfGuests,
                Payments = Enumerable.Range(0, numberOfGuests)
                                     .Select(_ => new GuestInvoice
                                     {
                                         AmountPaid = amountPerPerson,
                                         PaymentMethod = "Cash",
                                         Feedback = ""
                                     })
                                     .ToList()
            };
        }

        public (bool IsSuccess, string ErrorMessage) ProcessSplitPayment(SplitPaymentViewModel model)
        {
            try
            {
                // Validate total payment
                var totalPaid = model.Payments.Sum(p => Math.Round(p.AmountPaid, 2));
                var expected = Math.Round(model.TotalAmount, 2);

                if (Math.Abs(totalPaid - expected) > 0.01m) // Allow 1 cent difference for rounding
                {
                    return (false, $"❌ Total paid (€{totalPaid:0.00}) does not match the expected amount (€{expected:0.00}).");
                }

                // Create separate invoices for each guest
                foreach (var payment in model.Payments)
                {
                    if (payment.AmountPaid > 0)
                    {
                        _repo.SaveSplitPayment(model.OrderID, payment.AmountPaid, payment.PaymentMethod, payment.Feedback);
                    }
                }

                // Mark order as paid and free the table
                _repo.MarkOrderAsPaid(model.OrderID);
                _repo.FreeTable(model.TableID);

                return (true, "Split payment processed successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error processing split payment: {ex.Message}");
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tables = await _paymentService.GetTablesWithUnpaidOrdersAsync();
            return View("TableList", tables ?? new List<RestaurantTable>());
        }


        // Show full order details for a table
        [HttpGet]
        public async Task<IActionResult> ViewTableOrder(int tableId)
        {
            var viewModel = await _paymentService.GetCompleteOrderByTableIdAsync(tableId);

            if (viewModel == null || viewModel.Items == null || !viewModel.Items.Any())
            {
                ViewBag.Message = "No order found for this table.";
                return View("Empty");
            }

            return View("ViewTableOrder", viewModel);
        }

        // Show Finish Order form
        [HttpGet]
        public async Task<IActionResult> FinishOrder(int tableId)
        {
            int orderId = await _paymentService.GetOpenOrderIdByTableAsync(tableId);
            decimal total = await _paymentService.GetTotalAmountForTableAsync(tableId);
            decimal vat = await _paymentService.CalculateVATForOrderAsync(orderId);

            var model = new FinishOrderViewModel
            {
                TableID = tableId,
                OrderID = orderId,
                TotalAmount = total,
                VATAmount = vat
            };

            return View("FinishOrder", model);
        }

        // Handle Finish Order POST
        [HttpPost]
        public async Task<IActionResult> FinishOrder(FinishOrderViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var invoice = new Invoice
            {
                InvoiceNumber = Guid.NewGuid().ToString(),
                OrderID = model.OrderID,
                UserID = 1, // Replace with session/user context
                TotalAmount = model.TotalAmount,
                TipAmount = model.TipAmount,
                VATAmount = model.VATAmount
            };

            await _paymentService.CreateInvoiceAsync(invoice);
            await _paymentService.FreeTableAsync(model.TableID);

            TempData["Message"] = "✅ Order finished and table is now free!";
            return RedirectToAction("ViewTableOrder", new { tableId = model.TableID });
        }

        // Equal Split GET
        [HttpGet]
        
        public async Task<IActionResult> SplitPayment(int tableId, int numberOfGuests = 2)
        {
            if (numberOfGuests <= 0)
            {
                TempData["OrderStatus"] = "❌ Invalid number of guests.";
                return RedirectToAction("UnpaidTables"); // or another fallback
            }

            int orderId = await _paymentService.GetOpenOrderIdByTableAsync(tableId);
            if (orderId == 0)
            {
                TempData["OrderStatus"] = "❌ No open order found for this table.";
                return RedirectToAction("UnpaidTables");
            }

            decimal totalAmount = await _paymentService.GetTotalAmountForTableAsync(tableId);
            decimal amountPerGuest = Math.Round(totalAmount / numberOfGuests, 2);

            var model = new SplitPaymentViewModel
            {
                TableID = tableId,
                OrderID = orderId,
                TotalAmount = totalAmount,
                NumberOfGuests = numberOfGuests,
                Payments = Enumerable.Range(0, numberOfGuests)
                    .Select(i => new GuestPayment
                    {
                        AmountPaid = amountPerGuest
                    })
                    .ToList()
            };

            return View("SplitPayment", model);
        }


        // Handle Split POST
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SplitPayment(SplitPaymentViewModel model)
        {
            // Calculate total paid by all guests
            decimal totalPaid = model.Payments.Sum(p => p.AmountPaid);

            // Check if total paid is sufficient
            if (totalPaid < model.TotalAmount)
            {
                ModelState.AddModelError("", $"Guests paid €{totalPaid}, but the total is €{model.TotalAmount}.");
                return View(model);
            }

            // Save each guest payment
            foreach (var payment in model.Payments)
            {
                await _paymentService.SaveSplitPaymentAsync(
                    model.OrderID,
                    payment.AmountPaid,
                    payment.PaymentMethod,
                    payment.Feedback
                );
            }

            // ✅ Optional: Mark order as paid (recommended)
            await _paymentService.MarkOrderAsPaidAsync(model.OrderID);

            // ✅ Free the table
            await _paymentService.FreeTableAsync(model.TableID);

            // ✅ Success message
            TempData["Message"] = "✅ Split payment completed and table is now available.";

            // ✅ Redirect to main table list
            return RedirectToAction("Index"); // or "TableList" if your view is named that
        }

    }
}

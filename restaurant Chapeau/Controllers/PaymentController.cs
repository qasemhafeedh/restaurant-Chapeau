// Updated PaymentController.cs (No Business Logic, Synchronous Version)
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
        public IActionResult Index()
        {
            var tables = _paymentService.GetTablesWithUnpaidOrders();
            return View("TableList", tables);
        }

        [HttpGet]
        public IActionResult ViewTableOrder(int tableId)
        {
            var viewModel = _paymentService.GetCompleteOrderByTableId(tableId);
            if (viewModel == null || viewModel.Items == null || !viewModel.Items.Any())
            {
                return View("Empty");
            }
            return View("ViewTableOrder", viewModel);
        }

        [HttpGet]
        public IActionResult FinishOrder(int tableId)
        {
            var viewModel = _paymentService.BuildFinishOrderViewModel(tableId);
            return View("FinishOrder", viewModel);
        }

        [HttpPost]
        public IActionResult FinishOrder(FinishOrderViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = _paymentService.FinalizeOrder(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(model);
            }

            TempData["Message"] = "✅ Order finished and table is now free!";
            return RedirectToAction("Confirmation");

        }

        [HttpGet]
        public IActionResult SplitPayment(int tableId, int numberOfGuests = 2)
        {
            var model = _paymentService.BuildSplitPaymentViewModel(tableId, numberOfGuests);

            // TEMP DEBUG: Show amount in console
            Console.WriteLine("🚨 TotalAmount returned: " + model?.TotalAmount);

            if (model == null)
            {
                TempData["OrderStatus"] = "❌ No open order found for this table.";
                return RedirectToAction("Index");
            }

            return View("SplitPayment", model);
        }

        [HttpPost]
        public IActionResult SplitPayment(SplitPaymentViewModel model)
        {
            var result = _paymentService.ProcessSplitPayment(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(model);
            }

            return RedirectToAction("Confirmation");
        }


        [HttpGet]
        public IActionResult Confirmation()
        {
            return View();
        }
    }
}

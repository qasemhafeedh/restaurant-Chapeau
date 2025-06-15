using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace restaurant_Chapeau.Controllers
{
    public class FinancialController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public FinancialController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public IActionResult Overview(DateTime? start, DateTime? end)
        {
            DateTime startDate = start ?? DateTime.Today.AddMonths(-1); // default: last month
            DateTime endDate = end ?? DateTime.Today;

            List<Invoice> invoices = _invoiceService.GetInvoicesByDateRange(startDate, endDate);

            decimal totalRevenue = invoices.Sum(i => i.TotalAmount);
            decimal totalTips = invoices.Sum(i => i.TipAmount);
            decimal totalIncome = invoices.Sum(i => (i.TotalAmount - i.CostAmount));

            // Example classification: simple keyword check (can be replaced with smarter logic)
            decimal lunchSales = invoices.Where(i => i.InvoiceNumber.Contains("LUNCH")).Sum(i => i.TotalAmount);
            decimal dinnerSales = invoices.Where(i => i.InvoiceNumber.Contains("DINNER")).Sum(i => i.TotalAmount);
            decimal drinkSales = invoices.Where(i => i.InvoiceNumber.Contains("DRINK")).Sum(i => i.TotalAmount);

            decimal lunchIncome = invoices.Where(i => i.InvoiceNumber.Contains("LUNCH")).Sum(i => (i.TotalAmount - i.CostAmount));
            decimal dinnerIncome = invoices.Where(i => i.InvoiceNumber.Contains("DINNER")).Sum(i => (i.TotalAmount - i.CostAmount));
            decimal drinkIncome = invoices.Where(i => i.InvoiceNumber.Contains("DRINK")).Sum(i => (i.TotalAmount - i.CostAmount));

            ViewBag.StartDate = startDate.ToShortDateString();
            ViewBag.EndDate = endDate.ToShortDateString();
            ViewBag.LunchSales = lunchSales;
            ViewBag.DinnerSales = dinnerSales;
            ViewBag.DrinkSales = drinkSales;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalTips = totalTips;

            ViewBag.LunchIncome = lunchIncome;
            ViewBag.DinnerIncome = dinnerIncome;
            ViewBag.DrinkIncome = drinkIncome;
            ViewBag.TotalIncome = totalIncome;

            return View();
        }
    }
}
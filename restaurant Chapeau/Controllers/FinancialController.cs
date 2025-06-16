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

        public IActionResult Overview(DateTime? start, DateTime? end, string period) //? can be null
        {
            DateTime startDate, endDate;

            if (!string.IsNullOrEmpty(period))
            {
                endDate = DateTime.Today;

                switch (period.ToLower())
                {
                    case "month":
                        startDate = endDate.AddMonths(-1);
                        break;
                    case "quarter":
                        startDate = endDate.AddMonths(-3);
                        break;
                    case "year":
                        startDate = endDate.AddYears(-1);
                        break;
                    default:
                        startDate = end ?? DateTime.Today;
                        break;
                }
            }
            else
            {
                // Fallback to default 1 month
                startDate = start ?? DateTime.Today.AddMonths(-1);
                endDate = end ?? DateTime.Today;
            }


            var invoices = _invoiceService.GetInvoicesByDateRange(startDate, endDate);

            SetViewBagFinancials(invoices, startDate, endDate);

            return View();
        }

        private void SetViewBagFinancials(List<Invoice> invoices, DateTime startDate, DateTime endDate)
        {
            ViewBag.StartDate = startDate.ToShortDateString();
            ViewBag.EndDate = endDate.ToShortDateString();

            ViewBag.TotalRevenue = invoices.Sum(i => i.TotalAmount);
            ViewBag.TotalTips = invoices.Sum(i => i.TipAmount);
            ViewBag.TotalIncome = invoices.Sum(i => i.TotalAmount - i.CostAmount);

            (ViewBag.LunchSales, ViewBag.LunchIncome) = CalcByKeyword(invoices, "LUNCH");
            (ViewBag.DinnerSales, ViewBag.DinnerIncome) = CalcByKeyword(invoices, "DINNER");
            (ViewBag.DrinkSales, ViewBag.DrinkIncome) = CalcByKeyword(invoices, "DRINK");
        }

        private (decimal sales, decimal income) CalcByKeyword(List<Invoice> invoices, string keyword)
        {
            var items = invoices.Where(i => i.InvoiceNumber.Contains(keyword));
            return (items.Sum(i => i.TotalAmount), items.Sum(i => i.TotalAmount - i.CostAmount));
        }
    }
}
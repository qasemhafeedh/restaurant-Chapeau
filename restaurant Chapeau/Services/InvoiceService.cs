using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repo;

        public InvoiceService(IInvoiceRepository repo)
        {
            _repo = repo;
        }

        public List<Invoice> GetInvoicesByDateRange(DateTime start, DateTime end)
        {
            return _repo.GetInvoicesByDateRange(start, end);
        }
    }
}
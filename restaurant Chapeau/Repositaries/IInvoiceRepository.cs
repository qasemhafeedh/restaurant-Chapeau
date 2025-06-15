using restaurant_Chapeau.Models;
using System;
using System.Collections.Generic;

namespace restaurant_Chapeau.Repositaries
{
    public interface IInvoiceRepository
    {
        List<Invoice> GetInvoicesByDateRange(DateTime startDate, DateTime endDate);
    }
}
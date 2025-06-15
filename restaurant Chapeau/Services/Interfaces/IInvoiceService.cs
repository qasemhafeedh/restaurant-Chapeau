using restaurant_Chapeau.Models;
using System;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IInvoiceService
    {
        List<Invoice> GetInvoicesByDateRange(DateTime start, DateTime end);
    }
}
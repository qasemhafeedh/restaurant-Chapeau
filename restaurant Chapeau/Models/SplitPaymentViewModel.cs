namespace restaurant_Chapeau.Models
{
    public class SplitPaymentViewModel
    {
        public int TableID { get; set; }
        public int OrderID { get; set; }
        public decimal TotalAmount { get; set; }
        public int NumberOfGuests { get; set; }

        public List<GuestInvoice> Payments { get; set; } = new();
    }

    public class GuestInvoice
    {
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
    }
}


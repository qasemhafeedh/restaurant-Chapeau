namespace restaurant_Chapeau.Models
{
    public class SplitPaymentViewModel
    {
        public int TableID { get; set; }
        public int OrderID { get; set; }
        public decimal TotalAmount { get; set; }

        public int NumberOfGuests { get; set; } // ✅ This is required
        public List<GuestPayment> Payments { get; set; } = new(); // ✅ This too
    }

    public class SplitPaymentDetail
    {
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string Feedback { get; set; }
    }

}


namespace restaurant_Chapeau.Models
{
    public class FinishOrderViewModel
    {
        public int TableID { get; set; }
        public int OrderID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TipAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Feedback { get; set; }
        public decimal VATAmount { get; set; }
    }
}

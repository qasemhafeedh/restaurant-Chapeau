namespace restaurant_Chapeau.Models
{
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TipAmount { get; set; }
        public decimal VATAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        public decimal CostAmount { get; set; }
    }
}
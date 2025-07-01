using restaurant_Chapeau.ViewModels;
namespace restaurant_Chapeau.Models
{
    public class TableOrderView
    {
        public int OrderID { get; set; }                // ✅ Add this line
        public int TableID { get; set; }
        public int TableNumber { get; set; }
        public List<OrderItemView> Items { get; set; } = new();
        public decimal TotalLowVAT { get; set; }
        public decimal TotalHighVAT { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderID { get; set; }

    }

}


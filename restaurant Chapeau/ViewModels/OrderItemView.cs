namespace restaurant_Chapeau.ViewModels
{
    
        public class OrderItemView
        {
            public string ItemName { get; set; }
            public int Quantity { get; set; }
            public decimal TotalPrice { get; set; }
            public decimal VATRate { get; set; }
        }

    
}

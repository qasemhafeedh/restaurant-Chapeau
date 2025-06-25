namespace restaurant_Chapeau.Models
{
    public class CartItem
    {
        public int MenuItemID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; } = string.Empty;
        public string RoutingTarget { get; set; } = string.Empty;
        public decimal VATRate { get; set; }
    }
}


namespace restaurant_Chapeau.Models
{ // cart items ty p
    public class CartItem
    {
        public int MenuItemID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
        public string RoutingTarget { get; set; }
       
    }
}

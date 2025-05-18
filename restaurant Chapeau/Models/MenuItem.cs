namespace restaurant_Chapeau.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public bool IsAlcoholic { get; set; }
        public decimal VATRate { get; set; }
        public int QuantityAvailable { get; set; }
        public string MenuType { get; set; }
        public string RoutingTarget { get; set; }
    }
}

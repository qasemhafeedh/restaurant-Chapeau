using restaurant_Chapeau.Enums;

namespace restaurant_Chapeau.Models
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public string Name { get; set; }
		public Category Category { get; set; }
		public decimal Price { get; set; }
        public bool IsAlcoholic { get; set; }
        public decimal VATRate { get; set; }
        public int QuantityAvailable { get; set; }
		public MenuType MenuType { get; set; }
		public RoutingTarget RoutingTarget { get; set; }

		public bool IsActive { get; set; }
    }
}

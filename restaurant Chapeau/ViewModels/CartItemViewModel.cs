using restaurant_Chapeau.Enums;
namespace restaurant_Chapeau.ViewModels
{
    public class CartItemViewModel
    {
        public int MenuItemID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
        public RoutingTarget RoutingTarget { get; set; }

        public decimal Total => Price * Quantity;
    }

}

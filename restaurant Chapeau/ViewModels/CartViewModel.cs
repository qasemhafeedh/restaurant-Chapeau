
using restaurant_Chapeau.Models; 
namespace restaurant_Chapeau.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public int SelectedTableID { get; set; }
        public string Comment { get; set; }
        public decimal TipAmount { get; set; }

        public decimal Total => Items.Sum(item => item.Price * item.Quantity);

        // Add this to support tables in the view
        public List<RestaurantTable> Tables { get; set; } = new();
    }
}
    





using restaurant_Chapeau.Models; 
namespace restaurant_Chapeau.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public int SelectedTableID { get; set; }


        public decimal Total
        {
            get
            {
                decimal total = 0;
                foreach (CartItemViewModel item in Items)
                {
                    total += item.Price * item.Quantity;
                }
                return total;
            }
        }



    }
}
    




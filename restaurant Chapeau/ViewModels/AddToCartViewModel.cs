namespace restaurant_Chapeau.ViewModels
{
    public class AddToCartViewModel
    {
        public int MenuItemID { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
        public int TableID { get; set; } // This is required to know which table the order belongs to
    }

}

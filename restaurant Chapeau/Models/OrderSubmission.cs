namespace restaurant_Chapeau.Models
{
    public class OrderSubmission
    {
        public List<CartItem> CartItems { get; set; } = new();
        public int TableID { get; set; }
    
        public List<RestaurantTable> Tables { get; set; } = new(); // added here
    }
}


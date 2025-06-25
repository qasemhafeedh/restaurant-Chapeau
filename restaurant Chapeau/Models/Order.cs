namespace restaurant_Chapeau.Models
{
    public class Order
    {
        
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public List<OrderItem> Items { get; set; }
        public string comment { get; set; }
        public OrderStatus Status { get; set; }

    }
   
}

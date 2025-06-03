namespace restaurant_Chapeau.Models
{
    public class Orders
    {
        
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public List<OrderItems> Items { get; set; }
        public string comment { get; set; }
        public OrderStatus Status { get; set; }
        public class OrderItems
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public CourseType courseType { get; set; }
            public ItemStatus itemStatus { get; set; }
        }

    }
   
}

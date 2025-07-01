using restaurant_Chapeau.Enums;

namespace restaurant_Chapeau.Models
{
    public class Order
    {

        public int Id { get; set; }
        public int TableNumber { get; set; }
        public DateTime OrderTime { get; set; }
        public List<OrderItems> Items { get; set; }

        public OrderStatus Status { get; set; }
        /// <summary>
        /// the class above is created by Qasem. other things are for sultan?
        /// </summary>
        public class OrderItems
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string? Note { get; set; }
            public CourseType courseType { get; set; }
            public ItemStatus itemStatus { get; set; }
            public RoutingTarget target { get; set; }
            public MenuType menuType { get; set; }

        }
        public enum RoutingTarget
        {
            Kitchen,
            Bar
        }
        public Order(int id, int tableNumber, DateTime orderTime, List<OrderItems> items, string? comment,  OrderStatus status)
        {
            Id = id;
            TableNumber = tableNumber;
            OrderTime = orderTime;
            Items = items;
        
            Status = status;
        }

    }

}

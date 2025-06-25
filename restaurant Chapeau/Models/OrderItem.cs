using restaurant_Chapeau.Enums;
namespace restaurant_Chapeau.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public CourseType courseType { get; set; }
        public ItemStatus itemStatus { get; set; }
        public RoutingTarget target { get; set; }

    }
   
}

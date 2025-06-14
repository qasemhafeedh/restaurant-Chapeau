namespace restaurant_Chapeau.Models
{
    public class RestaurantTable
    {
        public int TableID { get; set; }
        public int TableNumber { get; set; }
        public bool IsReserved { get; set; }
        public DateTime? ReservationStart { get; set; }  //  NEW
        public DateTime? ReservationEnd { get; set; }    //  NEW
    }



}

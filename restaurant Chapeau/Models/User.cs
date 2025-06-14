namespace restaurant_Chapeau.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Store hashed passwords in production
        public string Role { get; set; } // "Manager" or "Waiter"
        public string FullName { get; set; }
    }

}

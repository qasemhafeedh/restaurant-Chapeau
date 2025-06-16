using restaurant_Chapeau.Enums;
using System.Data;

namespace restaurant_Chapeau.Models
{
	public class User
	{
		public int UserID { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
        public Role Role { get; set; }
        public string FullName { get; set; }
		public bool IsActive { get; set; } 
	}

}

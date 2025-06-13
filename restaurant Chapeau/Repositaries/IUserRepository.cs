using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositaries
{
	public interface IUserRepository
	{
		List<User> GetAllUsers();
		User GetUserById(int id);
		void AddUser(User user);
		void UpdateUser(User user);
		void ToggleActive(int id);
	}
}


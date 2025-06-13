using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Services.Interfaces
{
	public interface IUserService
	{

		List<User> GetAllUsers();
		User GetUserById(int id);
		void AddUser(User user);
		void UpdateUser(User user);
		void ToggleActive(int id);

	}
}

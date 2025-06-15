using global::restaurant_Chapeau.Models;
using global::restaurant_Chapeau.Repositaries;
using global::restaurant_Chapeau.Services.Interfaces;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Services.Interfaces;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepo;

		public UserService(IUserRepository userRepo)
		{
			_userRepo = userRepo;
		}

		public List<User> GetAllUsers()
		{
			return _userRepo.GetAllUsers();
		}

		public User GetUserById(int id)
		{
			return _userRepo.GetUserById(id);
		}

		public void AddUser(User user)
		{
			_userRepo.AddUser(user);
		}

		public void UpdateUser(User user)
		{
			_userRepo.UpdateUser(user);
		}

		public void ToggleActive(int id)
		{
			_userRepo.ToggleActive(id);
		}
	}
}


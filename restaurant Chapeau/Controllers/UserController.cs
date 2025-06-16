using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;
using System.Collections.Generic;

namespace restaurant_Chapeau.Controllers
{
	public class UserController : Controller
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		// Show all employees 
		public IActionResult ManageEmployees()
		{
			List<User> users = _userService.GetAllUsers();
			return View("ManageEmployees", users); 
		}

		// Show the Add form
		public IActionResult Add()
		{
			return View("AddEmployee");
		}

		//  Handle form submit (add)
		[HttpPost]
		public IActionResult Add(User user)
		{
			_userService.AddUser(user);
			return RedirectToAction("ManageEmployees");
		}

		// 4 Show Edit form
		public IActionResult Edit(int id)
		{
			User user = _userService.GetUserById(id);
			return View(user);
		}

		//  Save edit form
		[HttpPost]
		public IActionResult Edit(User user)
		{
			_userService.UpdateUser(user);
			return RedirectToAction("ManageEmployees");
		}

		// Activate or deactivate
		public IActionResult ToggleActive(int id)
		{
			_userService.ToggleActive(id);
			return RedirectToAction("ManageEmployees");
		}
	}
}
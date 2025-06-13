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

		// 1️⃣ Show all employees (renamed from Index)
		public IActionResult ManageEmployees()
		{
			List<User> users = _userService.GetAllUsers();
			return View("ManageEmployees", users); // This points to Views/User/ManageEmployees.cshtml
		}

		// 2️⃣ Show the Add form
		public IActionResult Add()
		{
			return View("AddEmployee");
		}

		// 3️⃣ Handle form submit (add)
		[HttpPost]
		public IActionResult Add(User user)
		{
			_userService.AddUser(user);
			return RedirectToAction("ManageEmployees");
		}

		// 4️⃣ Show Edit form
		public IActionResult Edit(int id)
		{
			User user = _userService.GetUserById(id);
			return View(user);
		}

		// 5️⃣ Save edit form
		[HttpPost]
		public IActionResult Edit(User user)
		{
			_userService.UpdateUser(user);
			return RedirectToAction("ManageEmployees");
		}

		// 6️⃣ Activate or deactivate
		public IActionResult ToggleActive(int id)
		{
			_userService.ToggleActive(id);
			return RedirectToAction("ManageEmployees");
		}
	}
}
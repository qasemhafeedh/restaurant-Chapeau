using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.AuthenticateAsync(model.Username, model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetInt32("UserID", user.UserID);

                return RedirectToAction("Menu", "Order");
            }

            TempData["LoginMessage"] = "Invalid username or password.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

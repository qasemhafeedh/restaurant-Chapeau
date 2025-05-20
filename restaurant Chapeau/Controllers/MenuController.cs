using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services;
using restaurant_Chapeau.Services.Interfaces;

public class MenuController : Controller
{
    private readonly IMenuService _menuService;
    public IActionResult ManageMenu()
    {
        var items = _menuService.GetAllItems(); // or _menuRepo
        return View(items); // Will load Views/Menu/ManageMenu.cshtml
    }
    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public IActionResult Add()
    {

        return View(); // Show the form
    }

    [HttpPost]
    public IActionResult Add(MenuItem item)
    {
        _menuService.AddItem(item); // Save to DB
        return RedirectToAction("ManageMenu");
    }
}
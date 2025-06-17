using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Enums;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services;
using restaurant_Chapeau.Services.Interfaces;
using System.Drawing;

public class MenuManagementController : Controller
{

    private readonly IMenuManagementService _menuService;
    public MenuManagementController(IMenuManagementService menuService)
    {
        _menuService = menuService;
    }

    public IActionResult ManageMenu(string menuType, string category)
    {
        Enum.TryParse(menuType, true, out MenuType MenuType);//when cant convert to enum it returns all
        Enum.TryParse(category, true, out Category Category);

        List<MenuItem> items = _menuService.GetAllItems(MenuType, Category);

        return View(items); 
    }
  
    [HttpGet]
    public IActionResult Add()
    {
        return View(); 
    }

    [HttpPost]
    public IActionResult Add(MenuItem item)
    {
        _menuService.AddItem(item); 
        return RedirectToAction("ManageMenu");
    }
    // Edit part
    [HttpGet]
    public IActionResult Edit(int id)
    {
        MenuItem item = _menuService.GetItemById(id);
        if (item == null)
        {
            return NotFound();
        }
        return View(item);
    }

    [HttpPost]
    public IActionResult Edit(MenuItem item)
    {
        _menuService.UpdateItem(item);
        return RedirectToAction("ManageMenu");
    }
    //Activate button
    public IActionResult ToggleActive(int id)
    {
        _menuService.ToggleActive(id);
        return RedirectToAction("ManageMenu");
    }
}
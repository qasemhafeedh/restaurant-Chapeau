using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services;
using restaurant_Chapeau.Services.Interfaces;

public class MenuManagementController : Controller
{

    private readonly IMenuManagementService _menuService;
    public MenuManagementController(IMenuManagementService menuService)
    {
        _menuService = menuService;
    }

    public IActionResult ManageMenu(string menuType, string category)
    {   
        List<MenuItem> items = _menuService.GetAllItems(menuType, category);

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
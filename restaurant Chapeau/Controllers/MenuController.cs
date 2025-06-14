﻿using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services;
using restaurant_Chapeau.Services.Interfaces;

public class MenuController : Controller
{
    private readonly IMenuService _menuService;

    public IActionResult ManageMenu(string menuType, string category)
    {   // ppass it down to the repository

        //  Get all menu items from the database
        List<MenuItem> items = _menuService.GetAllItems();

        //  Filter by Menu Type if selected
        if (!string.IsNullOrEmpty(menuType) && menuType != "All")
        {
            items = items.Where(item => item.MenuType == menuType).ToList();
        }

        //  Filter by Category if selected
        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            items = items.Where(item => item.Category == category).ToList();
        }

        //sort menu items. active comes first

        items = items.OrderByDescending(item => item.IsActive).ToList();

        return View(items);
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
    // this is the Edit menu Item part
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
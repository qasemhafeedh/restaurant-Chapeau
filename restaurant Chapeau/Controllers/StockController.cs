using Microsoft.AspNetCore.Mvc;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Services;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Controllers
{
    public class StockController : Controller
    {
        private readonly restaurant_Chapeau.Services.Interfaces.IStockService _stockservice;

        public StockController(IStockService stockService)
        {
            _stockservice = stockService;
        }

        // 1. Show all stock items
        public IActionResult ShowStock()
        {
            // var is array
            var items = _stockservice.GetAllItems();
            return View(items);
        }

        // 2. Show the edit page for one item
        public IActionResult Edit(int id)
        {
            if (id <= 0)
            {
                // If ID is missing or invalid, redirect to stock list. and Access by clicking
                return RedirectToAction("ShowStock");
            }
            var item = _stockservice.GetItemById(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // 3. Save the edited stock quantity
        [HttpPost]
        public IActionResult Edit(MenuItem model)
        {
            _stockservice.UpdateItem(model);
            return RedirectToAction("ShowStock");
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace restaurant_Chapeau.Controllers
{
    public class StockController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        //DbContext should be changed later when mergin codes
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var items = _context.MenuItems.ToList();
            return View(items);
        }
        // Show the edit page for one menu item
        public IActionResult Edit(int id)
        {
            var item = _context.MenuItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }
        //2. Save the new stock quantity
        [HttpPost]
        public IActionResult Edit(MenuItem model)
        {
            var item = _context.MenuItems.Find(model.MenuItemID);
            if (item != null)
            {
                item.QuantityAvailable = model.QuantityAvailable;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}

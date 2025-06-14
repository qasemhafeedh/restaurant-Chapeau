using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Services.Interfaces;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _repo;

        public StockService(IStockRepository repo)
        {
            _repo = repo;
        }

        public List<MenuItem> GetAllItems()
        {
            return _repo.GetAllItems();
        }

        public MenuItem GetItemById(int id)
        {
            return _repo.GetItemById(id);
        }

        public void UpdateItem(MenuItem item)
        {
            _repo.UpdateItem(item);
        }
    }
}

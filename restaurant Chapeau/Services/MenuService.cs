using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Services.Interfaces;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _repo;

        public MenuService(IMenuRepository repo)
        {
            _repo = repo;
        }

        public List<MenuItem> GetAllItems()
        {
            return _repo.GetAllItems();
        }

        public void AddItem(MenuItem item)
        {
            _repo.AddItem(item);
        }
    }
}
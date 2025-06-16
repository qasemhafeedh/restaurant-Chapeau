using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Repositories;
using restaurant_Chapeau.Services.Interfaces;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services
{
    public class MenuManagementService : IMenuManagementService
    {
        private readonly IMenuManagementRepository _repo;

        public MenuManagementService(IMenuManagementRepository repo)
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
        //EDIT
        public MenuItem GetItemById(int id)
        {
            return _repo.GetItemById(id);
        }

        public void UpdateItem(MenuItem item)
        {
            _repo.UpdateItem(item);
        }
        //Activate button
        public void ToggleActive(int id)
        {
            _repo.ToggleActive(id);
        }
    }
}
using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IMenuService
    {
        //add
        List<MenuItem> GetAllItems();
        void AddItem(MenuItem item);
       
        MenuItem GetItemById(int id);
        void UpdateItem(MenuItem item);

        //ACtivate button method
        void ToggleActive(int id);
    }
}
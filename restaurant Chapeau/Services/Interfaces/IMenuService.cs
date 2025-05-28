using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IMenuService
    { 
        //ADD
        List<MenuItem> GetAllItems();
        void AddItem(MenuItem item);
   

        //EDIT
        MenuItem GetItemById(int id);
        void UpdateItem(MenuItem item);

        //ACtivate button method
        void ToggleActive(int id);
    }
}
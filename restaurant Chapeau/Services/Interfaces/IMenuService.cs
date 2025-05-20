using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IMenuService
    {
        List<MenuItem> GetAllItems();
        void AddItem(MenuItem item);
        // Later: Add EditItem(), ToggleActive() etc.
    }
}
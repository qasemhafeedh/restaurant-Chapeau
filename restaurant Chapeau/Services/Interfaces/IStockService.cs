using restaurant_Chapeau.Models;
using System.Collections.Generic;

namespace restaurant_Chapeau.Services.Interfaces
{
    public interface IStockService
    {
        List<MenuItem> GetAllItems();
        MenuItem GetItemById(int id);
        void UpdateItem(MenuItem item);
    }
}
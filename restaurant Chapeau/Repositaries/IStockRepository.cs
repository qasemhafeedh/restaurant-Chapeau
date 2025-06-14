using System.Collections.Generic;
using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositories
{
    public interface IStockRepository
    {
        List<MenuItem> GetAllItems();
        MenuItem GetItemById(int id);
        void UpdateItem(MenuItem item);
    }
}
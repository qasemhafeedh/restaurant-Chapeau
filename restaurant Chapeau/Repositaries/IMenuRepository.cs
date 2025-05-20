using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositaries
{
    public  interface IMenuRepository
    {
        List<MenuItem> GetAllItems();
        void AddItem(MenuItem item);
        
    }
}

using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositaries
{
    public  interface IMenuManagementRepository
    {
        List<MenuItem> GetAllItems(string menuType = null, string category = null);
        void AddItem(MenuItem item);
        //EDIT
        MenuItem GetItemById(int id);
        void UpdateItem(MenuItem item);
        void ToggleActive(int id);
    }
}

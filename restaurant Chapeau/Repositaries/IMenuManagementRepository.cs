using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Repositaries
{
    public  interface IMenuManagementRepository
    {
        List<MenuItem> GetAllItems();
        void AddItem(MenuItem item);
        //EDIT
        MenuItem GetItemById(int id);
        void UpdateItem(MenuItem item);
        void ToggleActive(int id);
    }
}

using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;

     
        public TableService(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

     
        public Task<List<RestaurantTable>> GetAllTablesAsync()
        {
            return _tableRepository.GetAllTablesAsync();
        }

       
        public Task<bool> IsReservedAsync(int tableId)
        {
            return _tableRepository.IsReservedAsync(tableId);
        }

    
      



    }
}

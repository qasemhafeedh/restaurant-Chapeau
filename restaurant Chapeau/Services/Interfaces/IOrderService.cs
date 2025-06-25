using restaurant_Chapeau.Models;

namespace restaurant_Chapeau.Services.Interfaces
{
    
    
    public interface IOrderService
    {
        Task<bool> SubmitOrderAsync(OrderSubmission model, int userId);
       
    }


}

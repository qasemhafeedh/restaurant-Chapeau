using restaurant_Chapeau.Models;
using restaurant_Chapeau.ViewModels;

namespace restaurant_Chapeau.Services.Interfaces
{
    
    
    public interface IOrderService
    {
        Task<bool> SubmitOrderAsync(OrderSubmission model, int userId);
        Task<SubmitOrderResult> TrySubmitOrderAsync(CartViewModel model, int userId);

    }


}

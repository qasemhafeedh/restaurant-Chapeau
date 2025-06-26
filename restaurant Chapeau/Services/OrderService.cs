
using restaurant_Chapeau.Helpers;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;
using restaurant_Chapeau.ViewModels;

public class OrderService : IOrderService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITableService _tableService;
    private readonly IMenuItemService _menuItemService;
    private readonly IOrderRepository _orderRepository;

    public OrderService(
        ITableService tableService,
        IMenuItemService menuItemService,
        IOrderRepository orderRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _tableService = tableService;
        _menuItemService = menuItemService;
        _orderRepository = orderRepository;
        _httpContextAccessor = httpContextAccessor;
    }


    // check the cart before submitting 
    public async Task<bool> SubmitOrderAsync(OrderSubmission model, int userId)
    {
        if (await _tableService.IsReservedAsync(model.TableID))
            return false;

        foreach (var item in model.CartItems)
        {
            if (!await _menuItemService.IsStockAvailableAsync(item.MenuItemID, item.Quantity))
                throw new Exception($"Insufficient stock for {item.Name}");

            await _menuItemService.DecreaseStockAsync(item.MenuItemID, item.Quantity);
        }

        int orderId = await _orderRepository.CreateOrderAsync(model, userId);
        await _orderRepository.AddOrderItemsAsync(orderId, model.CartItems);

        return true;
    }
    public async Task<SubmitOrderResult> ProcessOrderSubmissionAsync(CartViewModel model, int userId)
    {
        if (model.SelectedTableID == 0)
            return new SubmitOrderResult { StatusMessage = "⚠️ Please select a table." };

        if (model.Items == null || !model.Items.Any())
            return new SubmitOrderResult { StatusMessage = "⚠️ Cart is empty." };

        //This converts the CartViewModel (used by the view/UserInterfacee) into a format (OrderSubmission) that the
        OrderSubmission order = new OrderSubmission
        {
            TableID = model.SelectedTableID,
            CartItems = MapToCartItems(model.Items),   // coversion
            TipAmount = model.TipAmount,
            Comment = model.Comment
        };

        bool submitted = await SubmitOrderAsync(order, userId);

        SubmitOrderResult result = new SubmitOrderResult();
        result.Success = submitted;

        if (submitted)
        {
            result.StatusMessage = "✅ Order submitted successfully!";
        }
        else
        {
            result.StatusMessage = " ";
        }

        return result;

    }

    //This method converts a list of cart items from the user interface (CartItemViewModel) into a list
    //of business-level objects (CartItem) used by the service/repository layer
    private List<CartItem> MapToCartItems(List<CartItemViewModel> items)
    {
        return items.Select(i => new CartItem
        {
            MenuItemID = i.MenuItemID,
            Name = i.Name,
            Price = i.Price,
            Quantity = i.Quantity,
            Note = i.Note,
            RoutingTarget = i.RoutingTarget.ToString()
        }).ToList();
    }


}

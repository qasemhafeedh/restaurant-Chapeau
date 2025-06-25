
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

        OrderSubmission order = new OrderSubmission
        {
            TableID = model.SelectedTableID,
            CartItems = model.Items.Select(i => new CartItem
            {
                MenuItemID = i.MenuItemID,
                Name = i.Name,
                Price = i.Price,
                Quantity = i.Quantity,
                Note = i.Note,
                RoutingTarget = i.RoutingTarget.ToString()
            }).ToList(),
            TipAmount = model.TipAmount,
            Comment = model.Comment
        };

        bool submitted = await SubmitOrderAsync(order, userId);

        return new SubmitOrderResult
        {
            Success = submitted,
            StatusMessage = submitted
                ? "✅ Order submitted successfully!"
                : "⚠️ Table is currently reserved."
        };
    }

}

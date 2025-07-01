
using restaurant_Chapeau.Helpers;
using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services;
using restaurant_Chapeau.Services.Interfaces;
using restaurant_Chapeau.ViewModels;

public class OrderService : IOrderService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITableService _tableService;
    private readonly IMenuItemService _menuItemService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;


    public OrderService(
        ITableService tableService,
        IMenuItemService menuItemService,
        IOrderRepository orderRepository,
        IHttpContextAccessor httpContextAccessor,
         ILogger<OrderService> logger)
    {
        _tableService = tableService;
        _menuItemService = menuItemService;
        _orderRepository = orderRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }


    // check the cart before submitting 
    public async Task<bool> SubmitOrderAsync(OrderSubmission model, int userId)
    {
        try
        {
            if (await _tableService.IsReservedAsync(model.TableID))
                throw new InvalidOperationException(" The selected table is currently reserved by another order.");

            foreach (var item in model.CartItems)
            {
                if (!await _menuItemService.IsStockAvailableAsync(item.MenuItemID, item.Quantity))
                    throw new InvalidOperationException($" Insufficient stock for item: {item.Name}");

                await _menuItemService.DecreaseStockAsync(item.MenuItemID, item.Quantity);
            }

            int orderId = await _orderRepository.CreateOrderAsync(model, userId);
            await _orderRepository.AddOrderItemsAsync(orderId, model.CartItems);

            return true;
        }
        catch (Exception ex)
        {
            // You can log this exception if desired
            throw new ApplicationException(" Failed to submit order.", ex);
        }
    }



    public async Task<SubmitOrderResult> ProcessOrderSubmissionAsync(CartViewModel model, int userId)
    {
        if(model.SelectedTableID == 0)
        throw new InvalidOperationException(" Table selection is required before submitting an order.");

        if (model.Items == null || !model.Items.Any())
            throw new InvalidOperationException(" Cannot submit an empty cart.");


        //This converts the CartViewModel (used by the view/UserInterfacee) into a format (OrderSubmission) that the
        OrderSubmission order = new OrderSubmission
        {
            TableID = model.SelectedTableID,
            CartItems = MapToCartItems(model.Items),   // coversion
           
        };

        bool submitted = await SubmitOrderAsync(order, userId);

        SubmitOrderResult result = new SubmitOrderResult();
        result.Success = submitted;

        if (submitted)
        {
            result.StatusMessage = " Order submitted successfully!";
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


    //////Because this logic is not about managing tables, it’s about managing the order process state, specifically tied to
    ///the user session and workflow. That’s why it belongs in OrderService.
    public bool IsTableSelected()
    {
        return _httpContextAccessor.HttpContext?.Session.GetInt32("SelectedTableID") != null;
    }

    public int GetSelectedTableId()
    {
        return _httpContextAccessor.HttpContext?.Session.GetInt32("SelectedTableID") ?? 0;
    }

    public string SetSelectedTableId(int tableId)
    {
        _httpContextAccessor.HttpContext?.Session.SetInt32("SelectedTableID", tableId);
        return $"✅ Table {tableId} selected.";
    }

    public void ClearOrderSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove("Cart");
        session?.Remove("SelectedTableID");
    }
    ////////////////////////(Below This Is for Kitchen And Bar)////////////////////////////////
    public List<Order> GetAllOrders(bool isKitchen, bool isReady)
    {
        List<Order> orders = _orderRepository.GetAllOrders(isKitchen, isReady);
        return orders;
    }

    public void UpdateOrderItemStatus(int orderId, int orderItemId, ItemStatus newStatus)
    {
        try
        {

            _orderRepository.UpdateOrderItemStatus(orderId, orderItemId, newStatus);
            _logger.LogInformation("Updated order item {ItemId} to status {Status}", orderItemId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order item status: {ItemId} to {Status}", orderItemId, newStatus);
            throw;
        }
    }

    public void UpdeteOrderStatus(Order order, OrderStatus newStatus)
    {
        try
        {
            _orderRepository.UpdateOrderStatus(order.Id, newStatus);
            _logger.LogInformation("Updated order {OrderId} to status {Status}", order.Id, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status: {OrderId} to {Status}", order.Id, newStatus);
            throw;
        }
    }

    public void UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus)
    {
        try
        {
            // Get the order to find all items for this course
            Order order = GetOrderById(orderId);
            if (order == null)
            {
                throw new ArgumentException($"Order with ID {orderId} not found");
            }

            // Find all items in the specified course
            var courseItems = order.Items.Where(item => item.courseType == courseType).ToList();

            if (!courseItems.Any())
            {
                _logger.LogWarning("No items found for course {CourseType} in order {OrderId}", courseType, orderId);
                return;
            }

            // Update each item in the course
            foreach (var item in courseItems)
            {
                _orderRepository.UpdateOrderItemStatus(order.Id, item.Id, newStatus);
            }

            _logger.LogInformation("Updated {ItemCount} items in course {CourseType} for order {OrderId} to status {Status}",
                courseItems.Count, courseType, orderId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course status: Order {OrderId}, Course {CourseType} to {Status}",
                orderId, courseType, newStatus);
            throw;
        }
    }

    public Order GetOrderById(int id)
    {
        try
        {
            var order = _orderRepository.GetOrderById(id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", id);
            }
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            throw;
        }
    }




}
